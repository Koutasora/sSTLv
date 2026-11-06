# Generuje ikona/sSTLv.ico (16-256 px, wpisy PNG) oraz ikona/podglad.png.
# Czysty Python, bez bibliotek: wielokaty rasteryzowane z nadprobkowaniem 4x4.
import os
import struct
import zlib

TLO = (0x3A, 0x6E, 0xA5)      # akcent
GORA = (0xF2, 0xF5, 0xF9)
LEWA = (0xC4, 0xCF, 0xDC)
PRAWA = (0x93, 0xA3, 0xB8)
ROZMIARY = [16, 24, 32, 48, 64, 128, 256]

# Geometria w ukladzie 256x256.
C = (128, 138)
SZESCIAN = [
    ([(128, 50), (206, 95), C, (50, 95)], GORA),
    ([(50, 95), C, (128, 228), (50, 183)], LEWA),
    ([C, (206, 95), (206, 183), (128, 228)], PRAWA),
]
POLE = (10, 10, 246, 246)
PROMIEN = 44


def w_wielokacie(x, y, pts):
    wew = False
    j = len(pts) - 1
    for i in range(len(pts)):
        xi, yi = pts[i]
        xj, yj = pts[j]
        if (yi > y) != (yj > y) and x < (xj - xi) * (y - yi) / (yj - yi) + xi:
            wew = not wew
        j = i
    return wew


def w_polu(x, y):
    x0, y0, x1, y1 = POLE
    if x < x0 or x > x1 or y < y0 or y > y1:
        return False
    cx = min(max(x, x0 + PROMIEN), x1 - PROMIEN)
    cy = min(max(y, y0 + PROMIEN), y1 - PROMIEN)
    return (x - cx) ** 2 + (y - cy) ** 2 <= PROMIEN ** 2


def kolor(x, y):
    for pts, k in SZESCIAN:
        if w_wielokacie(x, y, pts):
            return k
    if w_polu(x, y):
        return TLO
    return None


def renderuj(n, ss=4):
    skala = 256.0 / n
    wiersze = []
    for py in range(n):
        wiersz = bytearray()
        for px in range(n):
            r = g = b = a = 0
            for sy in range(ss):
                for sx in range(ss):
                    x = (px + (sx + 0.5) / ss) * skala
                    y = (py + (sy + 0.5) / ss) * skala
                    k = kolor(x, y)
                    if k:
                        r += k[0]; g += k[1]; b += k[2]; a += 1
            if a:
                wiersz += bytes((r // a, g // a, b // a, 255 * a // (ss * ss)))
            else:
                wiersz += b"\0\0\0\0"
        wiersze.append(bytes(wiersz))
    return wiersze


def png(n, wiersze):
    def blok(typ, dane):
        return struct.pack(">I", len(dane)) + typ + dane + struct.pack(">I", zlib.crc32(typ + dane) & 0xFFFFFFFF)
    surowe = b"".join(b"\0" + w for w in wiersze)
    return (b"\x89PNG\r\n\x1a\n" + blok(b"IHDR", struct.pack(">IIBBBBB", n, n, 8, 6, 0, 0, 0))
            + blok(b"IDAT", zlib.compress(surowe, 9)) + blok(b"IEND", b""))


def main():
    folder = os.path.dirname(os.path.abspath(__file__))
    obrazy = [(n, png(n, renderuj(n))) for n in ROZMIARY]
    naglowek = struct.pack("<HHH", 0, 1, len(obrazy))
    katalog = b""
    dane = b""
    offset = 6 + 16 * len(obrazy)
    for n, p in obrazy:
        katalog += struct.pack("<BBBBHHII", n % 256, n % 256, 0, 0, 1, 32, len(p), offset + len(dane))
        dane += p
    with open(os.path.join(folder, "sSTLv.ico"), "wb") as f:
        f.write(naglowek + katalog + dane)
    with open(os.path.join(folder, "podglad.png"), "wb") as f:
        f.write(dict(obrazy)[256])
    print("zapisano sSTLv.ico i podglad.png w " + folder)


if __name__ == "__main__":
    main()
