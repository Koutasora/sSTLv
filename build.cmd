@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%~dp0bin" mkdir "%~dp0bin"

"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /warn:4 /codepage:65001 /out:"%~dp0bin\sSTLv.exe" /win32icon:"%~dp0ikona\sSTLv.ico" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "%~dp0src\*.cs"
if errorlevel 1 (
  echo BLAD KOMPILACJI
  exit /b 1
)

"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /warn:4 /codepage:65001 /out:"%~dp0bin\sSTLv-instalator.exe" /win32icon:"%~dp0ikona\sSTLv.ico" /win32manifest:"%~dp0instalator\app.manifest" /resource:"%~dp0bin\sSTLv.exe",sSTLv.exe /reference:System.dll /reference:System.Windows.Forms.dll "%~dp0instalator\*.cs"
if errorlevel 1 (
  echo BLAD KOMPILACJI INSTALATORA
  exit /b 1
)

echo OK: %~dp0bin\sSTLv.exe
echo OK: %~dp0bin\sSTLv-instalator.exe
