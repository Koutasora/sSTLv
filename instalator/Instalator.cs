using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SstlvInstalator
{
    // Instalator per-user (HKCU, bez uprawnien administratora). Te same wpisy co instaluj.ps1.
    static class Program
    {
        const string ProgId = "sSTLv.stl";
        const string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\sSTLv";
        const string UserChoiceKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.stl\UserChoice";

        [DllImport("shell32.dll")]
        static extern void SHChangeNotify(int eventId, uint flags, IntPtr item1, IntPtr item2);

        static string InstallDir()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\sSTLv");
            if (!Path.IsPathRooted(dir)) throw new InvalidOperationException("Nie można ustalić folderu LocalAppData.");
            dir = Path.GetFullPath(dir);
            if (!dir.EndsWith(@"\Programs\sSTLv", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Nieoczekiwany folder instalacji: " + dir);
            return dir;
        }

        [STAThread]
        static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            try
            {
                bool uninstall = args.Length > 0 && string.Equals(args[0], "/odinstaluj", StringComparison.OrdinalIgnoreCase);
                if (uninstall) return Uninstall();
                return Install();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "sSTLv", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }

        static bool IsRunning()
        {
            Process[] p = Process.GetProcessesByName("sSTLv");
            bool running = p.Length > 0;
            foreach (Process x in p) x.Dispose();
            return running;
        }

        static void SetStr(string sub, string name, string value)
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(sub))
                k.SetValue(name, value, RegistryValueKind.String);
        }

        static void SetDword(string sub, string name, int value)
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(sub))
                k.SetValue(name, value, RegistryValueKind.DWord);
        }

        static void Notify()
        {
            SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
        }

        // Zwraca null gdy UserChoice nie istnieje, w przeciwnym razie ProgId (lub "").
        static string UserChoiceProgId()
        {
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(UserChoiceKey))
            {
                if (k == null) return null;
                object v = k.GetValue("ProgId");
                return v == null ? "" : v.ToString();
            }
        }

        static int Install()
        {
            if (IsRunning())
            {
                MessageBox.Show("Zamknij sSTLv i uruchom instalator ponownie.", "sSTLv", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 1;
            }

            string dir = InstallDir();
            string exe = Path.Combine(dir, "sSTLv.exe");
            string setup = Path.Combine(dir, "sSTLv-instalator.exe");
            Directory.CreateDirectory(dir);

            Assembly me = Assembly.GetExecutingAssembly();
            using (Stream rs = me.GetManifestResourceStream("sSTLv.exe"))
            {
                if (rs == null) throw new InvalidOperationException("Brak osadzonego sSTLv.exe.");
                using (FileStream fs = new FileStream(exe, FileMode.Create, FileAccess.Write, FileShare.None))
                    rs.CopyTo(fs);
            }

            string self = me.Location;
            if (!string.Equals(Path.GetFullPath(self), Path.GetFullPath(setup), StringComparison.OrdinalIgnoreCase))
                File.Copy(self, setup, true);

            string cmd = "\"" + exe + "\" \"%1\"";
            SetStr(@"Software\Classes\sSTLv.stl", "", "Model STL");
            SetStr(@"Software\Classes\sSTLv.stl\DefaultIcon", "", exe + ",0");
            SetStr(@"Software\Classes\sSTLv.stl\shell\open\command", "", cmd);
            SetStr(@"Software\Classes\.stl\OpenWithProgids", ProgId, "");
            SetStr(@"Software\Classes\Applications\sSTLv.exe", "FriendlyAppName", "sSTLv");
            SetStr(@"Software\Classes\Applications\sSTLv.exe\SupportedTypes", ".stl", "");
            SetStr(@"Software\Classes\Applications\sSTLv.exe\shell\open\command", "", cmd);
            SetStr(@"Software\sSTLv\Capabilities", "ApplicationName", "sSTLv");
            SetStr(@"Software\sSTLv\Capabilities", "ApplicationDescription", "Simple STL Viewer");
            SetStr(@"Software\sSTLv\Capabilities\FileAssociations", ".stl", ProgId);
            SetStr(@"Software\RegisteredApplications", "sSTLv", @"Software\sSTLv\Capabilities");

            // Bez UserChoice mozna ustawic domyslny program wprost; UserChoice nigdy nie jest ruszany.
            string chosen = UserChoiceProgId();
            if (chosen == null)
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(@"Software\Classes\.stl"))
                {
                    string cur = k.GetValue("") as string;
                    if (string.IsNullOrEmpty(cur) || cur == ProgId)
                        k.SetValue("", ProgId, RegistryValueKind.String);
                }
            }

            Version v = new Version(FileVersionInfo.GetVersionInfo(exe).FileVersion);
            string ver = v.ToString(3);
            int sizeKb = (int)((new FileInfo(exe).Length + new FileInfo(setup).Length) / 1024);

            SetStr(UninstallKey, "DisplayName", "sSTLv");
            SetStr(UninstallKey, "DisplayVersion", ver);
            SetStr(UninstallKey, "DisplayIcon", exe);
            SetStr(UninstallKey, "InstallLocation", dir);
            SetStr(UninstallKey, "UninstallString", "\"" + setup + "\" /odinstaluj");
            SetDword(UninstallKey, "NoModify", 1);
            SetDword(UninstallKey, "NoRepair", 1);
            SetDword(UninstallKey, "EstimatedSize", sizeKb);

            Notify();

            string msg = "Zainstalowano sSTLv " + ver + ".";
            bool needChoice = chosen != null && chosen != ProgId;
            if (needChoice) msg += "\nWybierz sSTLv dla plików .stl w oknie Ustawień.";
            MessageBox.Show(msg, "sSTLv", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (needChoice)
                Process.Start(new ProcessStartInfo("ms-settings:defaultapps?registeredAppUser=sSTLv") { UseShellExecute = true });
            return 0;
        }

        static void DeleteTree(string sub)
        {
            Registry.CurrentUser.DeleteSubKeyTree(sub, false);
        }

        static int Uninstall()
        {
            if (MessageBox.Show("Odinstalować sSTLv?", "sSTLv", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return 0;
            if (IsRunning())
            {
                MessageBox.Show("Zamknij sSTLv i uruchom odinstalowanie ponownie.", "sSTLv", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 1;
            }

            DeleteTree(@"Software\Classes\sSTLv.stl");
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.stl\OpenWithProgids", true))
                if (k != null) k.DeleteValue(ProgId, false);
            DeleteTree(@"Software\Classes\Applications\sSTLv.exe");
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\RegisteredApplications", true))
                if (k != null) k.DeleteValue("sSTLv", false);
            DeleteTree(@"Software\sSTLv");
            DeleteTree(UninstallKey);

            // Domyslna wartosc .stl tylko jesli wskazuje na nas.
            using (RegistryKey k = Registry.CurrentUser.OpenSubKey(@"Software\Classes\.stl", true))
                if (k != null && (k.GetValue("") as string) == ProgId)
                    k.SetValue("", "", RegistryValueKind.String);

            string dir = InstallDir();
            string exe = Path.Combine(dir, "sSTLv.exe");
            if (File.Exists(exe)) File.Delete(exe);

            Notify();
            MessageBox.Show("Odinstalowano sSTLv.", "sSTLv", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Folder zawiera dzialajacy instalator, wiec usuwa go osobny proces po zakonczeniu tego.
            if (Directory.Exists(dir))
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe",
                    "/c ping 127.0.0.1 -n 3 > nul & rmdir /s /q \"" + dir + "\"");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.WorkingDirectory = Path.GetTempPath();
                Process.Start(psi);
            }
            return 0;
        }
    }
}
