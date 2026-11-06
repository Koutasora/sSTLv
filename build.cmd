@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%~dp0bin" mkdir "%~dp0bin"
"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /warn:4 /codepage:65001 /out:"%~dp0bin\stl-podglad.exe" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "%~dp0src\*.cs"
if errorlevel 1 (
  echo BLAD KOMPILACJI
  exit /b 1
)
echo OK: %~dp0bin\stl-podglad.exe
