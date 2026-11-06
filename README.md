# sSTLv

Simple STL Viewer for Windows. C# 5, WinForms, OpenGL 1.1, no dependencies.

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

## Build

    build.cmd

Output: `bin\sSTLv.exe`, `bin\sSTLv-instalator.exe`. Uses the .NET Framework `csc.exe` built into Windows.

## Install from source

    instaluj.cmd
    odinstaluj.cmd

## Other

- Test files: `python test\generuj.py`
- Icon: `powershell -ExecutionPolicy Bypass -File ikona\generuj_ikone.ps1`
