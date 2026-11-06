# Generuje ikona\sSTLv.ico (16-48 px jako DIB, 64-256 px jako PNG) i ikona\podglad.png.
# Napis "sSTLv" od 48 px, sama litera "s" w mniejszych rozmiarach. Tylko System.Drawing.
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'

$folder = Split-Path -Parent $MyInvocation.MyCommand.Path
$tlo = [Drawing.Color]::FromArgb(255, 0x3A, 0x6E, 0xA5)
$rozmiary = 16, 24, 32, 48, 64, 128, 256
$czcionka = 'Segoe UI'

function Rysuj([int]$n) {
    $bmp = New-Object Drawing.Bitmap $n, $n, ([Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $g.TextRenderingHint = 'AntiAliasGridFit'
    $g.Clear([Drawing.Color]::Transparent)

    # Zaokraglony kwadrat
    $m = [Math]::Max(1.0, $n * 0.04)
    $r = $n * 0.18
    $w = $n - 2 * $m
    $p = New-Object Drawing.Drawing2D.GraphicsPath
    $p.AddArc($m, $m, 2 * $r, 2 * $r, 180, 90)
    $p.AddArc($m + $w - 2 * $r, $m, 2 * $r, 2 * $r, 270, 90)
    $p.AddArc($m + $w - 2 * $r, $m + $w - 2 * $r, 2 * $r, 2 * $r, 0, 90)
    $p.AddArc($m, $m + $w - 2 * $r, 2 * $r, 2 * $r, 90, 90)
    $p.CloseFigure()
    $g.FillPath((New-Object Drawing.SolidBrush $tlo), $p)

    # Napis jako sciezka, skalowany do szerokosci pola
    if ($n -ge 48) { $tekst = 'sSTLv'; $szer = 0.80; $wys = 0.50 } else { $tekst = 's'; $szer = 0.70; $wys = 0.78 }
    $t = New-Object Drawing.Drawing2D.GraphicsPath
    $f = New-Object Drawing.FontFamily $czcionka
    $t.AddString($tekst, $f, [int][Drawing.FontStyle]::Bold, 100, (New-Object Drawing.PointF 0, 0), [Drawing.StringFormat]::GenericTypographic)
    $b = $t.GetBounds()
    $s = [Math]::Min($n * $szer / $b.Width, $n * $wys / $b.Height)
    $mx = New-Object Drawing.Drawing2D.Matrix
    $mx.Translate($n / 2.0, $n / 2.0)
    $mx.Scale($s, $s)
    $mx.Translate(-($b.X + $b.Width / 2.0), -($b.Y + $b.Height / 2.0))
    $t.Transform($mx)
    $g.FillPath([Drawing.Brushes]::White, $t)

    $g.Dispose()
    $ms = New-Object IO.MemoryStream
    if ($n -ge 64) {
        $bmp.Save($ms, [Drawing.Imaging.ImageFormat]::Png)
    } else {
        # Klasyczny wpis DIB: BITMAPINFOHEADER, piksele BGRA od dolu, maska AND (zera).
        $bw = New-Object IO.BinaryWriter $ms
        $maskRow = [int]([Math]::Ceiling($n / 32.0) * 4)
        $bw.Write([UInt32]40); $bw.Write([Int32]$n); $bw.Write([Int32](2 * $n))
        $bw.Write([UInt16]1); $bw.Write([UInt16]32); $bw.Write([UInt32]0)
        $bw.Write([UInt32]($n * $n * 4 + $maskRow * $n))
        $bw.Write([Int32]0); $bw.Write([Int32]0); $bw.Write([UInt32]0); $bw.Write([UInt32]0)
        $rect = New-Object Drawing.Rectangle 0, 0, $n, $n
        $d = $bmp.LockBits($rect, 'ReadOnly', ([Drawing.Imaging.PixelFormat]::Format32bppArgb))
        $px = New-Object byte[] ($n * $n * 4)
        [Runtime.InteropServices.Marshal]::Copy($d.Scan0, $px, 0, $px.Length)
        $bmp.UnlockBits($d)
        for ($y = $n - 1; $y -ge 0; $y--) { $bw.Write($px, $y * $n * 4, $n * 4) }
        $bw.Write((New-Object byte[] ($maskRow * $n)))
        $bw.Flush()
    }
    $bmp.Dispose()
    return ,$ms.ToArray()
}

$obrazy = @()
foreach ($n in $rozmiary) { $obrazy += ,@($n, (Rysuj $n)) }

$out = New-Object IO.MemoryStream
$bw = New-Object IO.BinaryWriter $out
$bw.Write([UInt16]0); $bw.Write([UInt16]1); $bw.Write([UInt16]$obrazy.Count)
$offset = 6 + 16 * $obrazy.Count
foreach ($o in $obrazy) {
    $n = $o[0]; $png = $o[1]
    $bw.Write([byte]($n % 256)); $bw.Write([byte]($n % 256)); $bw.Write([byte]0); $bw.Write([byte]0)
    $bw.Write([UInt16]1); $bw.Write([UInt16]32); $bw.Write([UInt32]$png.Length); $bw.Write([UInt32]$offset)
    $offset += $png.Length
}
foreach ($o in $obrazy) { $bw.Write([byte[]]$o[1]) }
$bw.Flush()
[IO.File]::WriteAllBytes((Join-Path $folder 'sSTLv.ico'), $out.ToArray())
[IO.File]::WriteAllBytes((Join-Path $folder 'podglad.png'), $obrazy[$obrazy.Count - 1][1])
Write-Host ('zapisano sSTLv.ico i podglad.png w ' + $folder)
