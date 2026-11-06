# sSTLv

Simple STL Viewer for Windows 10/11. C# 5, WinForms, OpenGL 1.1.

## Download

[Releases](https://github.com/Koutasora/sSTLv/releases):

- `sSTLv-instalator.exe` - installer
- `sSTLv-portable.exe` - portable

## Controls

| Input | Action |
|---|---|
| Left / Right | previous / next .stl in folder |
| D | toggle 2D / 3D |
| Esc | quit |
| Wheel | zoom |
| Drag (2D) | pan |
| Left drag (3D) | rotate |
| Right / middle drag (3D) | pan |

Open a file: double-click (after install), drag & drop onto the window, or `sSTLv.exe file.stl`.

## Build

Requires Inno Setup 6 for the installer.

    build.cmd

Output: `bin\sSTLv.exe`, `bin\sSTLv-instalator.exe`.

## Other

- Test files: `python test\generuj.py`
- Icon: `powershell -ExecutionPolicy Bypass -File ikona\generuj_ikone.ps1`
