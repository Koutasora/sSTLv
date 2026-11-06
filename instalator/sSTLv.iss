; Instalator sSTLv (Inno Setup 6), per-user, bez uprawnien administratora. Wpisy HKCU jak w instaluj.ps1.
#define ExeFile "..\bin\sSTLv.exe"
#define FullVer GetVersionNumbersString(ExeFile)
#define AppVer Copy(FullVer, 1, RPos(".", FullVer) - 1)

[Setup]
AppId={{B05D25BB-AB3D-4BDE-9E47-1F24A96A9412}
AppName=sSTLv
AppVersion={#AppVer}
AppVerName=sSTLv {#AppVer}
VersionInfoVersion={#FullVer}
VersionInfoProductName=Simple STL Viewer
PrivilegesRequired=lowest
DefaultDirName={userpf}\sSTLv
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableWelcomePage=yes
DisableReadyPage=yes
ChangesAssociations=yes
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\sSTLv.exe
SetupIconFile=..\ikona\sSTLv.ico
OutputDir=..\bin
OutputBaseFilename=sSTLv-instalator
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ShowLanguageDialog=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"

[CustomMessages]
english.ChooseDefault=Choose sSTLv as the default app for .stl files
polish.ChooseDefault=Wybierz sSTLv jako domyślną aplikację dla plików .stl

[Files]
Source: "{#ExeFile}"; DestDir: "{app}"; Flags: ignoreversion

[Registry]
Root: HKA; Subkey: "Software\Classes\sSTLv.stl"; ValueType: string; ValueName: ""; ValueData: "Model STL"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\sSTLv.stl\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\sSTLv.exe,0"
Root: HKA; Subkey: "Software\Classes\sSTLv.stl\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\sSTLv.exe"" ""%1"""
Root: HKA; Subkey: "Software\Classes\.stl\OpenWithProgids"; ValueType: string; ValueName: "sSTLv.stl"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\Applications\sSTLv.exe"; ValueType: string; ValueName: "FriendlyAppName"; ValueData: "sSTLv"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\Applications\sSTLv.exe\SupportedTypes"; ValueType: string; ValueName: ".stl"; ValueData: ""
Root: HKA; Subkey: "Software\Classes\Applications\sSTLv.exe\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\sSTLv.exe"" ""%1"""
Root: HKA; Subkey: "Software\sSTLv\Capabilities"; ValueType: string; ValueName: "ApplicationName"; ValueData: "sSTLv"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\sSTLv\Capabilities"; ValueType: string; ValueName: "ApplicationDescription"; ValueData: "Simple STL Viewer"
Root: HKA; Subkey: "Software\sSTLv\Capabilities\FileAssociations"; ValueType: string; ValueName: ".stl"; ValueData: "sSTLv.stl"
Root: HKA; Subkey: "Software\RegisteredApplications"; ValueType: string; ValueName: "sSTLv"; ValueData: "Software\sSTLv\Capabilities"; Flags: uninsdeletevalue

[Run]
Filename: "ms-settings:defaultapps?registeredAppUser=sSTLv"; Description: "{cm:ChooseDefault}"; Flags: postinstall shellexec skipifsilent nowait; Check: NeedsDefaultChoice

[Code]
// Windows chroni wybor domyslnej aplikacji (UserChoice) - nie ruszamy go, tylko prosimy uzytkownika o wybor.
function NeedsDefaultChoice: Boolean;
var
  ProgId: String;
begin
  Result := not (RegQueryStringValue(HKCU,
    'Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.stl\UserChoice', 'ProgId', ProgId)
    and (ProgId = 'sSTLv.stl'));
end;

// Migracja ze starego instalatora C#: jego wpis w "Aplikacjach zainstalowanych" i kopia instalatora.
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    RegDeleteKeyIncludingSubkeys(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\sSTLv');
    DeleteFile(ExpandConstant('{app}\sSTLv-instalator.exe'));
  end;
end;
