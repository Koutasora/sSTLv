using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StlPodglad
{
    // Kontrolka OpenGL 1.1. Rysuje wylacznie na zdarzenia (Paint), bez timerow i petli.
    class GLView : Control
    {
        const double Fov = 40.0;
        const double BasePitch = -60.0;
        const double BaseYaw = -45.0;

        IntPtr hdc = IntPtr.Zero;
        IntPtr hrc = IntPtr.Zero;

        // Model: bufor VBO (OpenGL 1.5) albo, gdy sterownik go nie ma, tablica po stronie klienta.
        delegate void GenBuffersFn(int n, ref uint buffers);
        delegate void BindBufferFn(int target, uint buffer);
        delegate void BufferDataFn(int target, IntPtr size, IntPtr data, int usage);
        delegate void DeleteBuffersFn(int n, ref uint buffers);
        GenBuffersFn glGenBuffers;
        BindBufferFn glBindBuffer;
        BufferDataFn glBufferData;
        DeleteBuffersFn glDeleteBuffers;

        uint vbo;
        int vertCount;
        float[] cpuData;
        GCHandle cpuPin;
        double cx, cy, cz;          // srodek bryly
        double hx, hy, hz;          // polowy wymiarow
        double radius;              // promien kuli opisanej

        bool dark = true;
        bool mode3D;
        double zoom = 1.0;
        double panX, panY;          // piksele, y w gore, wzgledem srodka okna
        double yaw = BaseYaw;
        double pitch = BasePitch;

        int lastX, lastY;
        bool trimmedOnce, trimPending;

        public GLView()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.Selectable, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            AllowDrop = true;
            TabStop = false;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= 0x20;                  // CS_OWNDC
                cp.Style |= 0x02000000 | 0x04000000;    // WS_CLIPCHILDREN | WS_CLIPSIBLINGS
                return cp;
            }
        }

        public bool Dark
        {
            get { return dark; }
            set { dark = value; Invalidate(); }
        }

        public bool Mode3D
        {
            get { return mode3D; }
            set { mode3D = value; ResetView(); }
        }

        public bool HasModel
        {
            get { return vertCount != 0; }
        }

        public void ResetView()
        {
            zoom = 1.0;
            panX = 0; panY = 0;
            yaw = BaseYaw;
            pitch = BasePitch;
            Invalidate();
        }

        // Wczytuje plik; zwraca false przy bledzie (widok pusty).
        public bool LoadFile(string path)
        {
            if (hrc == IntPtr.Zero) return false;
            Native.wglMakeCurrent(hdc, hrc);
            ReleaseModel();

            bool ok = false;
            try
            {
                StlModel m = StlLoader.Load(path);
                Upload(m);
                cx = (m.MinX + m.MaxX) * 0.5; cy = (m.MinY + m.MaxY) * 0.5; cz = (m.MinZ + m.MaxZ) * 0.5;
                hx = (m.MaxX - m.MinX) * 0.5; hy = (m.MaxY - m.MinY) * 0.5; hz = (m.MaxZ - m.MinZ) * 0.5;
                radius = Math.Sqrt(hx * hx + hy * hy + hz * hz);
                if (radius < 1e-9) radius = 1.0;
                ok = true;
            }
            catch (System.IO.IOException) { }
            catch (System.IO.InvalidDataException) { }
            catch (UnauthorizedAccessException) { }
            catch (ArgumentException) { }
            catch (NotSupportedException) { }
            catch (InvalidOperationException) { }
            catch (OutOfMemoryException) { }

            ResetView();
            GC.Collect();
            if (!trimmedOnce)
            {
                trimmedOnce = true;
                trimPending = true;
            }
            return ok;
        }

        void ReleaseModel()
        {
            if (vbo != 0)
            {
                glDeleteBuffers(1, ref vbo);
                vbo = 0;
            }
            if (cpuPin.IsAllocated) cpuPin.Free();
            cpuData = null;
            vertCount = 0;
        }

        T Proc<T>(string name) where T : class
        {
            IntPtr p = Native.wglGetProcAddress(name);
            if (p == IntPtr.Zero || (long)p == 1 || (long)p == 2 || (long)p == 3 || (long)p == -1) return null;
            return (T)(object)Marshal.GetDelegateForFunctionPointer(p, typeof(T));
        }

        void LoadBufferFunctions()
        {
            glGenBuffers = Proc<GenBuffersFn>("glGenBuffers");
            glBindBuffer = Proc<BindBufferFn>("glBindBuffer");
            glBufferData = Proc<BufferDataFn>("glBufferData");
            glDeleteBuffers = Proc<DeleteBuffersFn>("glDeleteBuffers");
            if (glGenBuffers == null || glBindBuffer == null || glBufferData == null || glDeleteBuffers == null)
                glGenBuffers = null;
        }

        void Upload(StlModel m)
        {
            if (glGenBuffers != null)
            {
                GCHandle pin = GCHandle.Alloc(m.Data, GCHandleType.Pinned);
                try
                {
                    glGenBuffers(1, ref vbo);
                    glBindBuffer(Native.GL_ARRAY_BUFFER, vbo);
                    glBufferData(Native.GL_ARRAY_BUFFER, (IntPtr)((long)m.Floats * 4), pin.AddrOfPinnedObject(), Native.GL_STATIC_DRAW);
                    glBindBuffer(Native.GL_ARRAY_BUFFER, 0);
                }
                finally { pin.Free(); }
            }
            else
            {
                cpuData = m.Data;
                cpuPin = GCHandle.Alloc(cpuData, GCHandleType.Pinned);
            }
            vertCount = m.Floats / 6;
        }

        void DrawBuffer()
        {
            IntPtr basePtr = IntPtr.Zero;
            if (vbo != 0) glBindBuffer(Native.GL_ARRAY_BUFFER, vbo);
            else basePtr = cpuPin.AddrOfPinnedObject();
            Native.glEnableClientState(Native.GL_NORMAL_ARRAY);
            Native.glEnableClientState(Native.GL_VERTEX_ARRAY);
            Native.glNormalPointer(Native.GL_FLOAT, 24, basePtr);
            Native.glVertexPointer(3, Native.GL_FLOAT, 24, (IntPtr)((long)basePtr + 12));
            Native.glDrawArrays(Native.GL_TRIANGLES, 0, vertCount);
            Native.glDisableClientState(Native.GL_NORMAL_ARRAY);
            Native.glDisableClientState(Native.GL_VERTEX_ARRAY);
            if (vbo != 0) glBindBuffer(Native.GL_ARRAY_BUFFER, 0);
        }

        // Jednorazowe przyciecie working set po pierwszym namalowaniu (nie przy kazdej zmianie pliku).
        public void TrimAfterFirstPaint()
        {
            trimmedOnce = true;
            trimPending = true;
            Invalidate();
        }

        static void TrimWorkingSet()
        {
            Native.SetProcessWorkingSetSize(Native.GetCurrentProcess(), (IntPtr)(-1), (IntPtr)(-1));
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            hdc = Native.GetDC(Handle);

            PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)System.Runtime.InteropServices.Marshal.SizeOf(typeof(PIXELFORMATDESCRIPTOR));
            pfd.nVersion = 1;
            pfd.dwFlags = 0x04 | 0x20 | 0x01;   // DRAW_TO_WINDOW | SUPPORT_OPENGL | DOUBLEBUFFER
            pfd.iPixelType = 0;                 // RGBA
            pfd.cColorBits = 32;
            pfd.cDepthBits = 24;

            int fmt = Native.ChoosePixelFormat(hdc, ref pfd);
            if (fmt == 0 || !Native.SetPixelFormat(hdc, fmt, ref pfd)) return;
            hrc = Native.wglCreateContext(hdc);
            if (hrc == IntPtr.Zero) return;
            Native.wglMakeCurrent(hdc, hrc);
            LoadBufferFunctions();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ReleaseModel();
            if (hrc != IntPtr.Zero)
            {
                Native.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                Native.wglDeleteContext(hrc);
                hrc = IntPtr.Zero;
            }
            if (hdc != IntPtr.Zero)
            {
                Native.ReleaseDC(Handle, hdc);
                hdc = IntPtr.Zero;
            }
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (hrc == IntPtr.Zero) return;
            int w = ClientSize.Width, h = ClientSize.Height;
            if (w < 1 || h < 1) return;

            Native.wglMakeCurrent(hdc, hrc);
            Native.glViewport(0, 0, w, h);
            if (dark) Native.glClearColor(0x1E / 255f, 0x1E / 255f, 0x1E / 255f, 1f);
            else Native.glClearColor(0xF2 / 255f, 0xF2 / 255f, 0xF2 / 255f, 1f);
            Native.glClear(Native.GL_COLOR_BUFFER_BIT | Native.GL_DEPTH_BUFFER_BIT);

            if (vertCount != 0) DrawModel(w, h);
            Native.SwapBuffers(hdc);
            if (trimPending)
            {
                trimPending = false;
                BeginInvoke(new MethodInvoker(TrimWorkingSet));
            }
        }

        // Skala 2D (piksele na jednostke), tak by rzut bryly w stalym kacie zmiescil sie w oknie.
        double FitScale2D(int w, int h)
        {
            double pr = BasePitch * Math.PI / 180.0, yr = BaseYaw * Math.PI / 180.0;
            double cy_ = Math.Cos(yr), sy = Math.Sin(yr), cp = Math.Cos(pr), sp = Math.Sin(pr);
            double ex = 0, ey = 0;
            for (int i = 0; i < 8; i++)
            {
                double x = (i & 1) != 0 ? hx : -hx;
                double y = (i & 2) != 0 ? hy : -hy;
                double z = (i & 4) != 0 ? hz : -hz;
                double x1 = x * cy_ - y * sy;
                double y1 = x * sy + y * cy_;
                double y2 = y1 * cp - z * sp;
                ex = Math.Max(ex, Math.Abs(x1));
                ey = Math.Max(ey, Math.Abs(y2));
            }
            ex = Math.Max(ex, radius * 1e-6);
            ey = Math.Max(ey, radius * 1e-6);
            return Math.Min(w * 0.5 / ex, h * 0.5 / ey) * 0.92;
        }

        void SetupLights()
        {
            Native.glEnable(Native.GL_LIGHTING);
            Native.glEnable(Native.GL_NORMALIZE);
            Native.glEnable(Native.GL_DEPTH_TEST);
            Native.glShadeModel(Native.GL_FLAT);
            Native.glLightModeli(Native.GL_LIGHT_MODEL_TWO_SIDE, 1);
            Native.glLightModelfv(Native.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.22f, 0.22f, 0.22f, 1f });
            Native.glEnable(Native.GL_COLOR_MATERIAL);
            Native.glColorMaterial(Native.GL_FRONT_AND_BACK, Native.GL_AMBIENT_AND_DIFFUSE);

            // Swiatla w ukladzie kamery (modelview = jednostkowa w chwili ustawiania).
            Native.glEnable(Native.GL_LIGHT0);
            Native.glLightfv(Native.GL_LIGHT0, Native.GL_POSITION, new float[] { -0.4f, 0.7f, 0.9f, 0f });
            Native.glLightfv(Native.GL_LIGHT0, Native.GL_DIFFUSE, new float[] { 0.85f, 0.85f, 0.85f, 1f });
            Native.glLightfv(Native.GL_LIGHT0, Native.GL_AMBIENT, new float[] { 0f, 0f, 0f, 1f });
            Native.glEnable(Native.GL_LIGHT1);
            Native.glLightfv(Native.GL_LIGHT1, Native.GL_POSITION, new float[] { 0.8f, -0.3f, 0.3f, 0f });
            Native.glLightfv(Native.GL_LIGHT1, Native.GL_DIFFUSE, new float[] { 0.35f, 0.35f, 0.35f, 1f });
            Native.glLightfv(Native.GL_LIGHT1, Native.GL_AMBIENT, new float[] { 0f, 0f, 0f, 1f });
        }

        void DrawModel(int w, int h)
        {
            Native.glMatrixMode(Native.GL_PROJECTION);
            Native.glLoadIdentity();
            Native.glMatrixMode(Native.GL_MODELVIEW);
            Native.glLoadIdentity();

            if (!mode3D)
            {
                double s = FitScale2D(w, h) * zoom;
                double depth = radius * s * 1.5 + 1.0;
                Native.glMatrixMode(Native.GL_PROJECTION);
                Native.glOrtho(-w * 0.5, w * 0.5, -h * 0.5, h * 0.5, -depth, depth);
                Native.glMatrixMode(Native.GL_MODELVIEW);
                SetupLights();
                Native.glTranslated(panX, panY, 0);
                Native.glScaled(s, s, s);
            }
            else
            {
                double tanHalf = Math.Tan(Fov * 0.5 * Math.PI / 180.0);
                double aspect = (double)w / h;
                double halfMin = Math.Atan(tanHalf * Math.Min(1.0, aspect));
                double d0 = radius / Math.Sin(halfMin) * 1.05;
                double d = d0 / zoom;
                double near = d * 0.01;
                double far = d + radius * 2.0;
                double top = near * tanHalf;
                Native.glMatrixMode(Native.GL_PROJECTION);
                Native.glFrustum(-top * aspect, top * aspect, -top, top, near, far);
                Native.glMatrixMode(Native.GL_MODELVIEW);
                SetupLights();
                double upp = 2.0 * d * tanHalf / h;
                Native.glTranslated(panX * upp, panY * upp, -d);
            }

            Native.glRotated(pitch, 1, 0, 0);
            Native.glRotated(yaw, 0, 0, 1);
            Native.glTranslated(-cx, -cy, -cz);

            if (dark) Native.glColor3f(0.62f, 0.68f, 0.76f);
            else Native.glColor3f(0.36f, 0.43f, 0.53f);
            DrawBuffer();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            lastX = e.X; lastY = e.Y;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.None) return;
            int dx = e.X - lastX, dy = e.Y - lastY;
            lastX = e.X; lastY = e.Y;
            if (vertCount == 0) return;

            if (mode3D && e.Button == MouseButtons.Left)
            {
                yaw += dx * 0.5;
                pitch = Math.Max(-180.0, Math.Min(0.0, pitch + dy * 0.5));
            }
            else
            {
                panX += dx;
                panY -= dy;
            }
            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (vertCount == 0) return;
            double nz = Math.Max(0.02, Math.Min(500.0, zoom * Math.Pow(1.15, e.Delta / 120.0)));
            double f = nz / zoom;
            double mx = e.X - ClientSize.Width * 0.5;
            double my = ClientSize.Height * 0.5 - e.Y;
            panX = mx - (mx - panX) * f;
            panY = my - (my - panY) * f;
            zoom = nz;
            Invalidate();
        }
    }
}
