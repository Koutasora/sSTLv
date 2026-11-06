using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace StlPodglad
{
    class MainForm : Form
    {
        const string Nazwa = "stl-podglad";

        readonly GLView view = new GLView();
        List<string> files = new List<string>();
        int index = -1;
        bool error;
        string startPath;

        public MainForm(string path)
        {
            startPath = path;
            Text = Nazwa;
            ClientSize = new System.Drawing.Size(960, 640);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;

            view.Dock = DockStyle.Fill;
            view.Dark = LoadTheme();
            view.DragEnter += OnDragEnter;
            view.DragDrop += OnDragDrop;
            Controls.Add(view);
            ActiveControl = view;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (startPath != null) OpenFile(startPath);
            else GLView.TrimMemory();
        }

        // Motyw z ustawien Windows (AppsUseLightTheme: 1 = jasny, 0 lub brak = ciemny).
        static bool LoadTheme()
        {
            try
            {
                object v = Microsoft.Win32.Registry.GetValue(
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                    "AppsUseLightTheme", null);
                if (v is int && (int)v == 1) return false;
            }
            catch (Exception) { }
            return true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (view.Dark)
            {
                try
                {
                    int one = 1;
                    Native.DwmSetWindowAttribute(Handle, 20, ref one, 4);
                }
                catch (Exception) { }
            }
        }

        static bool IsStl(string path)
        {
            return string.Equals(Path.GetExtension(path), ".stl", StringComparison.OrdinalIgnoreCase);
        }

        // Otwiera plik i odczytuje liste .stl z jego folderu.
        void OpenFile(string path)
        {
            List<string> list = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(Path.GetDirectoryName(path)))
                    if (IsStl(f)) list.Add(f);
            }
            catch (Exception) { }
            list.Sort(delegate(string a, string b)
            {
                return Native.StrCmpLogicalW(Path.GetFileName(a), Path.GetFileName(b));
            });

            int i = list.FindIndex(delegate(string f)
            {
                return string.Equals(f, path, StringComparison.OrdinalIgnoreCase);
            });
            if (i < 0)
            {
                list.Add(path);
                i = list.Count - 1;
            }
            files = list;
            index = i;
            LoadCurrent();
        }

        void LoadCurrent()
        {
            error = !view.LoadFile(files[index]);
            UpdateTitle();
        }

        void Step(int dir)
        {
            if (files.Count < 2) return;
            index = (index + dir + files.Count) % files.Count;
            LoadCurrent();
        }

        void UpdateTitle()
        {
            if (index < 0) { Text = Nazwa; return; }
            Text = Path.GetFileName(files[index]) + "  " + (index + 1) + "/" + files.Count
                + "  " + (view.Mode3D ? "3D" : "2D") + (error ? "  błąd odczytu" : "");
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left: Step(-1); return true;
                case Keys.Right: Step(1); return true;
                case Keys.D:
                    view.Mode3D = !view.Mode3D;
                    UpdateTitle();
                    return true;
                case Keys.Escape:
                    Close();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        static string FirstStl(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return null;
            string[] names = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (names == null) return null;
            foreach (string n in names)
                if (IsStl(n)) return n;
            return null;
        }

        void OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = FirstStl(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        void OnDragDrop(object sender, DragEventArgs e)
        {
            string p = FirstStl(e);
            if (p != null) OpenFile(p);
        }
    }
}
