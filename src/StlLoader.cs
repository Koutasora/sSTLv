using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace StlPodglad
{
    // Wynik wczytania: lista wyswietlania i bryla otaczajaca.
    class StlModel
    {
        public uint List;
        public float MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
    }

    // Czyta STL strumieniowo prosto do listy wyswietlania OpenGL (kontekst musi byc aktywny).
    static class StlLoader
    {
        class Acc : IDisposable
        {
            const int ChunkTris = 16384;
            readonly float[] verts = new float[ChunkTris * 9];
            readonly float[] norms = new float[ChunkTris * 9];
            GCHandle hv, hn;
            int pending;
            public int Count;
            public float MinX = float.MaxValue, MinY = float.MaxValue, MinZ = float.MaxValue;
            public float MaxX = float.MinValue, MaxY = float.MinValue, MaxZ = float.MinValue;

            public Acc()
            {
                hv = GCHandle.Alloc(verts, GCHandleType.Pinned);
                hn = GCHandle.Alloc(norms, GCHandleType.Pinned);
                Native.glEnableClientState(Native.GL_VERTEX_ARRAY);
                Native.glEnableClientState(Native.GL_NORMAL_ARRAY);
                Native.glVertexPointer(3, Native.GL_FLOAT, 0, hv.AddrOfPinnedObject());
                Native.glNormalPointer(Native.GL_FLOAT, 0, hn.AddrOfPinnedObject());
            }

            // Dane tablic sa kopiowane do listy w chwili glDrawArrays (kompilacji listy).
            public void Flush()
            {
                if (pending == 0) return;
                Native.glDrawArrays(Native.GL_TRIANGLES, 0, pending * 3);
                pending = 0;
            }

            public void Dispose()
            {
                Native.glDisableClientState(Native.GL_VERTEX_ARRAY);
                Native.glDisableClientState(Native.GL_NORMAL_ARRAY);
                hv.Free();
                hn.Free();
            }

            static bool Bad(float v)
            {
                return float.IsNaN(v) || float.IsInfinity(v);
            }

            public void Tri(float nx, float ny, float nz,
                float ax, float ay, float az, float bx, float by, float bz, float cx, float cy, float cz)
            {
                if (Bad(ax) || Bad(ay) || Bad(az) || Bad(bx) || Bad(by) || Bad(bz) || Bad(cx) || Bad(cy) || Bad(cz))
                    return;
                if (Bad(nx) || Bad(ny) || Bad(nz)) { nx = 0; ny = 0; nz = 0; }

                if (nx * nx + ny * ny + nz * nz < 1e-12f)
                {
                    float ux = bx - ax, uy = by - ay, uz = bz - az;
                    float vx = cx - ax, vy = cy - ay, vz = cz - az;
                    nx = uy * vz - uz * vy;
                    ny = uz * vx - ux * vz;
                    nz = ux * vy - uy * vx;
                    if (nx * nx + ny * ny + nz * nz < 1e-30f) { nx = 0; ny = 0; nz = 1; }
                }

                int o = pending * 9;
                for (int i = 0; i < 9; i += 3) { norms[o + i] = nx; norms[o + i + 1] = ny; norms[o + i + 2] = nz; }
                verts[o] = ax; verts[o + 1] = ay; verts[o + 2] = az;
                verts[o + 3] = bx; verts[o + 4] = by; verts[o + 5] = bz;
                verts[o + 6] = cx; verts[o + 7] = cy; verts[o + 8] = cz;
                if (++pending == ChunkTris) Flush();

                Extend(ax, ay, az);
                Extend(bx, by, bz);
                Extend(cx, cy, cz);
                Count++;
            }

            void Extend(float x, float y, float z)
            {
                if (x < MinX) MinX = x;
                if (y < MinY) MinY = y;
                if (z < MinZ) MinZ = z;
                if (x > MaxX) MaxX = x;
                if (y > MaxY) MaxY = y;
                if (z > MaxZ) MaxZ = z;
            }
        }

        // Zwraca model albo rzuca wyjatek (plik uszkodzony / pusty). Lista jest sprzatana przy bledzie.
        public static StlModel Load(string path)
        {
            uint list = Native.glGenLists(1);
            if (list == 0) throw new InvalidOperationException("glGenLists");
            Acc acc = null;
            bool ok = false;
            try
            {
                Native.glNewList(list, Native.GL_COMPILE);
                try
                {
                    acc = new Acc();
                    ReadFile(path, acc);
                    acc.Flush();
                }
                finally
                {
                    if (acc != null) acc.Dispose();
                    Native.glEndList();
                }

                if (acc.Count == 0) throw new InvalidDataException("brak trojkatow");
                ok = true;
            }
            finally
            {
                if (!ok) Native.glDeleteLists(list, 1);
            }

            StlModel m = new StlModel();
            m.List = list;
            m.MinX = acc.MinX; m.MinY = acc.MinY; m.MinZ = acc.MinZ;
            m.MaxX = acc.MaxX; m.MaxY = acc.MaxY; m.MaxZ = acc.MaxZ;
            return m;
        }

        static void ReadFile(string path, Acc acc)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 65536))
            {
                long size = fs.Length;
                if (size == 0) throw new InvalidDataException("pusty plik");

                byte[] head = new byte[84];
                int got = ReadFull(fs, head, 0, 84);

                // "solid" po opcjonalnym BOM UTF-8 i bialych znakach
                int off = 0;
                if (got >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF) off = 3;
                while (off < got && head[off] <= 0x20) off++;
                bool solid = got >= off + 5 && head[off] == (byte)'s' && head[off + 1] == (byte)'o'
                    && head[off + 2] == (byte)'l' && head[off + 3] == (byte)'i' && head[off + 4] == (byte)'d';

                long n = 0;
                bool sizeMatches = false;
                if (got == 84)
                {
                    n = BitConverter.ToUInt32(head, 80);
                    sizeMatches = size == 84 + 50 * n;
                }

                if (solid && !sizeMatches)
                {
                    fs.Position = 0;
                    try { ReadAscii(fs, acc); }
                    catch (InvalidDataException) { if (acc.Count > 0) throw; }
                    if (acc.Count > 0) return;
                    // Binarny plik z naglowkiem "solid" i nadmiarowymi bajtami na koncu.
                    if (n == 0 || size < 84 + 50 * n) throw new InvalidDataException("brak trojkatow");
                    fs.Position = 84;
                    ReadBinary(fs, n, acc);
                    return;
                }

                if (got < 84 || size < 84 + 50 * n) throw new InvalidDataException("za krotki plik");
                ReadBinary(fs, n, acc);
            }
        }

        static int ReadFull(Stream s, byte[] buf, int offset, int count)
        {
            int total = 0;
            while (total < count)
            {
                int r = s.Read(buf, offset + total, count - total);
                if (r <= 0) break;
                total += r;
            }
            return total;
        }

        static void ReadBinary(Stream s, long n, Acc acc)
        {
            const int Chunk = 4096;
            byte[] buf = new byte[50 * Chunk];
            long left = n;
            while (left > 0)
            {
                int take = (int)Math.Min(left, Chunk);
                int want = take * 50;
                if (ReadFull(s, buf, 0, want) != want) throw new InvalidDataException("uciety plik");
                for (int i = 0; i < take; i++)
                {
                    int o = i * 50;
                    acc.Tri(
                        BitConverter.ToSingle(buf, o), BitConverter.ToSingle(buf, o + 4), BitConverter.ToSingle(buf, o + 8),
                        BitConverter.ToSingle(buf, o + 12), BitConverter.ToSingle(buf, o + 16), BitConverter.ToSingle(buf, o + 20),
                        BitConverter.ToSingle(buf, o + 24), BitConverter.ToSingle(buf, o + 28), BitConverter.ToSingle(buf, o + 32),
                        BitConverter.ToSingle(buf, o + 36), BitConverter.ToSingle(buf, o + 40), BitConverter.ToSingle(buf, o + 44));
                }
                left -= take;
            }
        }

        static float Num(string s)
        {
            float v;
            if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                throw new InvalidDataException("zla liczba");
            return v;
        }

        static void ReadAscii(Stream s, Acc acc)
        {
            char[] sep = new char[] { ' ', '\t' };
            float nx = 0, ny = 0, nz = 0;
            float[] v = new float[9];
            int k = 0;
            using (StreamReader r = new StreamReader(s, Encoding.UTF8, true, 65536))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("facet", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] t = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        if (t.Length < 5) throw new InvalidDataException("zly facet");
                        nx = Num(t[2]); ny = Num(t[3]); nz = Num(t[4]);
                        k = 0;
                    }
                    else if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] t = line.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        if (t.Length < 4 || k >= 3) throw new InvalidDataException("zly vertex");
                        v[k * 3] = Num(t[1]); v[k * 3 + 1] = Num(t[2]); v[k * 3 + 2] = Num(t[3]);
                        k++;
                    }
                    else if (line.StartsWith("endfacet", StringComparison.OrdinalIgnoreCase))
                    {
                        if (k != 3) throw new InvalidDataException("facet bez 3 wierzcholkow");
                        acc.Tri(nx, ny, nz, v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7], v[8]);
                        k = 0;
                    }
                }
            }
        }
    }
}
