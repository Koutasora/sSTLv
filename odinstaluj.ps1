# Odinstalowanie sSTLv: usuwa tylko wpisy HKCU i folder utworzone przez instaluj.ps1.
# Plik zapisany w ASCII (Windows PowerShell 5.1 czyta skrypty bez BOM jako ANSI).
$ErrorActionPreference = 'Stop'

$dir = Join-Path $env:LOCALAPPDATA 'Programs\sSTLv'
$progId = 'sSTLv.stl'
$hkcu = [Microsoft.Win32.Registry]::CurrentUser

if (Get-Process -Name 'sSTLv' -ErrorAction SilentlyContinue) {
    Write-Host 'sSTLv jest uruchomiony. Zamknij program i uruchom odinstalowanie ponownie.'
    exit 1
}

$removed = @()

function Remove-Tree($subKey) {
    $k = $hkcu.OpenSubKey($subKey)
    if ($k -ne $null) {
        $k.Close()
        $hkcu.DeleteSubKeyTree($subKey)
        $script:removed += ('HKCU\' + $subKey)
    }
}

function Remove-Value($subKey, $name) {
    $k = $hkcu.OpenSubKey($subKey, $true)
    if ($k -ne $null) {
        try {
            if ($k.GetValue($name, $null) -ne $null) {
                $k.DeleteValue($name)
                $script:removed += ('HKCU\' + $subKey + ' : ' + $name)
            }
        } finally { $k.Close() }
    }
}

Remove-Tree 'Software\Classes\sSTLv.stl'
Remove-Value 'Software\Classes\.stl\OpenWithProgids' $progId
Remove-Tree 'Software\Classes\Applications\sSTLv.exe'
Remove-Value 'Software\RegisteredApplications' 'sSTLv'
Remove-Tree 'Software\sSTLv'

# Domyslna wartosc .stl tylko jesli wskazuje na nas.
$k = $hkcu.OpenSubKey('Software\Classes\.stl', $true)
if ($k -ne $null) {
    try {
        if ($k.GetValue('') -eq $progId) {
            $k.SetValue('', '', [Microsoft.Win32.RegistryValueKind]::String)
            $removed += 'HKCU\Software\Classes\.stl : (domyslna)'
        }
    } finally { $k.Close() }
}

if (Test-Path -LiteralPath $dir) {
    Remove-Item -LiteralPath $dir -Recurse -Force
    $removed += $dir
}

Add-Type -Namespace SSTLv -Name Shell -MemberDefinition '[System.Runtime.InteropServices.DllImport("shell32.dll")] public static extern void SHChangeNotify(int eventId, uint flags, System.IntPtr item1, System.IntPtr item2);'
[SSTLv.Shell]::SHChangeNotify(0x08000000, 0, [IntPtr]::Zero, [IntPtr]::Zero)

if ($removed.Count -eq 0) { Write-Host 'Nic do usuniecia.' }
else {
    Write-Host 'Usunieto:'
    foreach ($r in $removed) { Write-Host ('  ' + $r) }
}
