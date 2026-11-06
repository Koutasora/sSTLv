using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace StlPodglad
{
    // Wynik wczytania: przeplatane dane wierzcholkow (nx,ny,nz,x,y,z na wierzcholek,
    // 3 wierzcholki na trojkat) i bryla otaczajaca. Data moze byc dluzsze niz Floats.
    class StlModel
    {
        public float[] Data;
        public int Floats;
        public float MinX, MinY, MinZ, MaxX, MaxY, MaxZ;
    }

    // Czyta STL strumieniowo do jednej tablicy float (bez OpenGL).
    static class StlLoader
    {
        class Acc
        {
            public float[] Data = new float[18 * 1024];
            public int Floats;
            public int Count;
            public float MinX = float.MaxValue, MinY = float.MaxValue, MinZ = float.MaxValue;
            public float MaxX = float.MinValue, MaxY = float.MinValue, MaxZ = float.MinValue;

            public void Reserve(long floats)
            {
                if (floats > 0x7FFFFFC7) throw new OutOfMemoryException();
                if (floats > Data.Length) Data = new float[floats];
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

                if (Floats + 18 > Data.Length)
                {
                    long want = (long)Data.Length * 2;
                    if (want > 0x7FFFFFC7) want = 0x7FFFFFC7;
                    if (want < Floats + 18) throw new OutOfMemoryException();
                    float[] bigger = new float[want];
                    Array.Copy(Data, bigger, Floats);
                    Data = bigger;
                }
                float[] d = Data;
                int o = Floats;
                d[o] = nx; d[o + 1] = ny; d[o + 2] = nz; d[o + 3] = ax; d[o + 4] = ay; d[o + 5] = az;
                d[o + 6] = nx; d[o + 7] = ny; d[o + 8] = nz; d[o + 9] = bx; d[o + 10] = by; d[o + 11] = bz;
                d[o + 12] = nx; d[o + 13] = ny; d[o + 14] = nz; d[o + 15] = cx; d[o + 16] = cy; d[o + 17] = cz;
                Floats += 18;

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

        // Zwraca model albo rzuca wyjatek (plik uszkodzony / pusty).
        public static StlModel Load(string path)
        {
            Acc acc = new Acc();
            ReadFile(path, acc);
            if (acc.Count == 0) throw new InvalidDataException("brak trojkatow");

            StlModel m = new StlModel();
            m.Data = acc.Data;
            m.Floats = acc.Floats;
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
            acc.Reserve(n * 18);
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
            using (StreamReader r = new StreamReader(s, Encoding.UTF8, true, 65536, true))
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
