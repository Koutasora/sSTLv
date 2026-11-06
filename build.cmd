@echo off
setlocal
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%~dp0bin" mkdir "%~dp0bin"

"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /warn:4 /codepage:65001 /out:"%~dp0bin\sSTLv.exe" /win32icon:"%~dp0ikona\sSTLv.ico" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll "%~dp0src\*.cs"
if errorlevel 1 (
  echo BLAD KOMPILACJI
  exit /b 1
)
echo OK: %~dp0bin\sSTLv.exe

set ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" set ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe
if not exist "%ISCC%" (
  echo Pominieto instalator: brak Inno Setup 6 ^(ISCC.exe^).
  exit /b 0
)

"%ISCC%" /Q "%~dp0instalator\sSTLv.iss"
if errorlevel 1 (
  echo BLAD KOMPILACJI INSTALATORA
  exit /b 1
)
echo OK: %~dp0bin\sSTLv-instalator.exe
