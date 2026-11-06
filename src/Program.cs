using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StlPodglad
{
    static class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main(string[] args)
        {
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string start = null;
            if (args.Length > 0)
            {
                try { start = Path.GetFullPath(args[0]); }
                catch (ArgumentException) { }
                catch (NotSupportedException) { }
                catch (PathTooLongException) { }
            }
            Application.Run(new MainForm(start));
        }
    }
}
