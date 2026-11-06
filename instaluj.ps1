# Instalacja sSTLv dla biezacego uzytkownika (HKCU, bez uprawnien administratora).
# Plik zapisany w ASCII (Windows PowerShell 5.1 czyta skrypty bez BOM jako ANSI).
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $root 'bin\sSTLv.exe'
$dir = Join-Path $env:LOCALAPPDATA 'Programs\sSTLv'
$exe = Join-Path $dir 'sSTLv.exe'
$progId = 'sSTLv.stl'

if (-not (Test-Path $src)) {
    Write-Host 'Brak bin\sSTLv.exe, buduje...'
    & (Join-Path $root 'build.cmd')
    if (-not (Test-Path $src)) { Write-Host 'Budowanie nie powiodlo sie.'; exit 1 }
}

if (Get-Process -Name 'sSTLv' -ErrorAction SilentlyContinue) {
    Write-Host 'sSTLv jest uruchomiony. Zamknij program i uruchom instalacje ponownie.'
    exit 1
}

New-Item -ItemType Directory -Force -Path $dir | Out-Null
Copy-Item -Force $src $exe

function Set-Reg($subKey, $name, $value) {
    $k = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($subKey)
    try {
        if ($value -eq $null) { $k.SetValue($name, '', [Microsoft.Win32.RegistryValueKind]::String) }
        else { $k.SetValue($name, $value, [Microsoft.Win32.RegistryValueKind]::String) }
    } finally { $k.Close() }
}

$cmd = '"' + $exe + '" "%1"'

Set-Reg 'Software\Classes\sSTLv.stl' '' 'Model STL'
Set-Reg 'Software\Classes\sSTLv.stl\DefaultIcon' '' ($exe + ',0')
Set-Reg 'Software\Classes\sSTLv.stl\shell\open\command' '' $cmd

Set-Reg 'Software\Classes\.stl\OpenWithProgids' $progId $null

Set-Reg 'Software\Classes\Applications\sSTLv.exe' 'FriendlyAppName' 'sSTLv'
Set-Reg 'Software\Classes\Applications\sSTLv.exe\SupportedTypes' '.stl' $null
Set-Reg 'Software\Classes\Applications\sSTLv.exe\shell\open\command' '' $cmd

Set-Reg 'Software\sSTLv\Capabilities' 'ApplicationName' 'sSTLv'
Set-Reg 'Software\sSTLv\Capabilities' 'ApplicationDescription' 'Simple STL Viewer'
Set-Reg 'Software\sSTLv\Capabilities\FileAssociations' '.stl' $progId
Set-Reg 'Software\RegisteredApplications' 'sSTLv' 'Software\sSTLv\Capabilities'

# Bez UserChoice mozna ustawic domyslny program wprost; UserChoice nigdy nie jest ruszany.
$userChoice = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.stl\UserChoice'
$chosen = $null
if (Test-Path -LiteralPath $userChoice) {
    $chosen = (Get-ItemProperty -LiteralPath $userChoice -ErrorAction SilentlyContinue).ProgId
    if ($chosen -eq $null) { $chosen = '' }
}

$setDefault = $false
if ($chosen -eq $null) {
    $k = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey('Software\Classes\.stl')
    try {
        $cur = $k.GetValue('')
        if ([string]::IsNullOrEmpty($cur) -or $cur -eq $progId) {
            $k.SetValue('', $progId, [Microsoft.Win32.RegistryValueKind]::String)
            $setDefault = $true
        }
    } finally { $k.Close() }
}

Add-Type -Namespace SSTLv -Name Shell -MemberDefinition '[System.Runtime.InteropServices.DllImport("shell32.dll")] public static extern void SHChangeNotify(int eventId, uint flags, System.IntPtr item1, System.IntPtr item2);'
[SSTLv.Shell]::SHChangeNotify(0x08000000, 0, [IntPtr]::Zero, [IntPtr]::Zero)

$ver = (Get-Item $exe).VersionInfo.FileVersion
Write-Host ('Zainstalowano sSTLv ' + $ver + ' w ' + $dir)

if ($chosen -ne $null -and $chosen -ne $progId) {
    Write-Host 'Windows wymaga recznego wyboru: kliknij "Ustaw domyslne" dla sSTLv.'
    Start-Process 'ms-settings:defaultapps?registeredAppUser=sSTLv'
} elseif ($setDefault) {
    Write-Host 'Ustawiono sSTLv jako domyslny program dla .stl.'
}
