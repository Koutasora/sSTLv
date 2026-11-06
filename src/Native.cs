using System;
using System.Runtime.InteropServices;
using System.Security;

namespace StlPodglad
{
    [StructLayout(LayoutKind.Sequential)]
    struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits, cRedShift, cGreenBits, cGreenShift, cBlueBits, cBlueShift;
        public byte cAlphaBits, cAlphaShift;
        public byte cAccumBits, cAccumRedBits, cAccumGreenBits, cAccumBlueBits, cAccumAlphaBits;
        public byte cDepthBits, cStencilBits, cAuxBuffers, iLayerType, bReserved;
        public uint dwLayerMask, dwVisibleMask, dwDamageMask;
    }

    [SuppressUnmanagedCodeSecurity]
    static class Native
    {
        public const int GL_DEPTH_TEST = 0x0B71;
        public const int GL_LIGHTING = 0x0B50;
        public const int GL_LIGHT0 = 0x4000;
        public const int GL_LIGHT1 = 0x4001;
        public const int GL_NORMALIZE = 0x0BA1;
        public const int GL_COLOR_MATERIAL = 0x0B57;
        public const int GL_LIGHT_MODEL_TWO_SIDE = 0x0B52;
        public const int GL_LIGHT_MODEL_AMBIENT = 0x0B53;
        public const int GL_POSITION = 0x1203;
        public const int GL_DIFFUSE = 0x1201;
        public const int GL_AMBIENT = 0x1200;
        public const int GL_AMBIENT_AND_DIFFUSE = 0x1602;
        public const int GL_FRONT_AND_BACK = 0x0408;
        public const int GL_ARRAY_BUFFER = 0x8892;
        public const int GL_STATIC_DRAW = 0x88E4;
        public const int GL_TRIANGLES = 0x0004;
        public const int GL_PROJECTION = 0x1701;
        public const int GL_MODELVIEW = 0x1700;
        public const int GL_COLOR_BUFFER_BIT = 0x4000;
        public const int GL_DEPTH_BUFFER_BIT = 0x0100;
        public const int GL_FLAT = 0x1D00;
        public const int GL_FLOAT = 0x1406;
        public const int GL_VERTEX_ARRAY = 0x8074;
        public const int GL_NORMAL_ARRAY = 0x8075;

        const string GL = "opengl32.dll";

        [DllImport(GL)] public static extern void glClearColor(float r, float g, float b, float a);
        [DllImport(GL)] public static extern void glClear(int mask);
        [DllImport(GL)] public static extern void glEnable(int cap);
        [DllImport(GL)] public static extern void glViewport(int x, int y, int w, int h);
        [DllImport(GL)] public static extern void glMatrixMode(int mode);
        [DllImport(GL)] public static extern void glLoadIdentity();
        [DllImport(GL)] public static extern void glOrtho(double l, double r, double b, double t, double n, double f);
        [DllImport(GL)] public static extern void glFrustum(double l, double r, double b, double t, double n, double f);
        [DllImport(GL)] public static extern void glTranslated(double x, double y, double z);
        [DllImport(GL)] public static extern void glScaled(double x, double y, double z);
        [DllImport(GL)] public static extern void glRotated(double a, double x, double y, double z);
        [DllImport(GL)] public static extern void glLightfv(int light, int pname, float[] p);
        [DllImport(GL)] public static extern void glLightModelfv(int pname, float[] p);
        [DllImport(GL)] public static extern void glLightModeli(int pname, int param);
        [DllImport(GL)] public static extern void glColorMaterial(int face, int mode);
        [DllImport(GL)] public static extern void glColor3f(float r, float g, float b);
        [DllImport(GL)] public static extern void glShadeModel(int mode);
        [DllImport(GL)] public static extern void glEnableClientState(int array);
        [DllImport(GL)] public static extern void glDisableClientState(int array);
        [DllImport(GL)] public static extern void glVertexPointer(int size, int type, int stride, IntPtr p);
        [DllImport(GL)] public static extern void glNormalPointer(int type, int stride, IntPtr p);
        [DllImport(GL)] public static extern void glDrawArrays(int mode, int first, int count);

        [DllImport(GL)] public static extern IntPtr wglCreateContext(IntPtr hdc);
        [DllImport(GL)] public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);
        [DllImport(GL)] public static extern bool wglDeleteContext(IntPtr hglrc);
        [DllImport(GL)] public static extern IntPtr wglGetProcAddress(string name);

        [DllImport("gdi32.dll")] public static extern int ChoosePixelFormat(IntPtr hdc, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] public static extern bool SetPixelFormat(IntPtr hdc, int format, ref PIXELFORMATDESCRIPTOR pfd);
        [DllImport("gdi32.dll")] public static extern bool SwapBuffers(IntPtr hdc);

        [DllImport("user32.dll")] public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string a, string b);

        [DllImport("dwmapi.dll")] public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        [DllImport("kernel32.dll")] public static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")] public static extern bool SetProcessWorkingSetSize(IntPtr proc, IntPtr min, IntPtr max);
    }
}
