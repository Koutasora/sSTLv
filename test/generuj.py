"""Generuje male pliki testowe STL do test/pliki/."""
import os
import struct

OUT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "pliki")
os.makedirs(OUT, exist_ok=True)


def sub(a, b):
    return (a[0] - b[0], a[1] - b[1], a[2] - b[2])


def cross(u, v):
    return (u[1] * v[2] - u[2] * v[1], u[2] * v[0] - u[0] * v[2], u[0] * v[1] - u[1] * v[0])


def norm(n):
    d = (n[0] ** 2 + n[1] ** 2 + n[2] ** 2) ** 0.5
    return (n[0] / d, n[1] / d, n[2] / d) if d else (0.0, 0.0, 1.0)


def quad(a, b, c, d):
    """Czworokat (CCW patrzac z zewnatrz) -> dwa trojkaty."""
    return [(a, b, c), (a, c, d)]


def cube(s=10.0):
    p = [(x, y, z) for z in (0, s) for y in (0, s) for x in (0, s)]
    # indeks = x + 2*y + 4*z
    q = lambda i, j, k, l: quad(p[i], p[j], p[k], p[l])
    tris = []
    tris += q(0, 2, 3, 1)  # dol
    tris += q(4, 5, 7, 6)  # gora
    tris += q(0, 1, 5, 4)  # przod (y=0)
    tris += q(2, 6, 7, 3)  # tyl (y=s)
    tris += q(0, 4, 6, 2)  # lewo (x=0)
    tris += q(1, 3, 7, 5)  # prawo (x=s)
    return tris


def pyramid(s=10.0, h=12.0):
    a, b, c, d = (0, 0, 0), (s, 0, 0), (s, s, 0), (0, s, 0)
    t = (s / 2, s / 2, h)
    tris = quad(a, d, c, b)  # podstawa, normalna w dol
    tris += [(a, b, t), (b, c, t), (c, d, t), (d, a, t)]
    return tris


def octahedron(r=8.0):
    px, nx = (r, 0, 0), (-r, 0, 0)
    py, ny = (0, r, 0), (0, -r, 0)
    pz, nz = (0, 0, r), (0, 0, -r)
    tris = []
    for z in (pz, nz):
        pairs = [(px, py), (py, nx), (nx, ny), (ny, px)]
        for u, v in pairs:
            tris.append((v, u, z) if z is nz else (z, u, v))
    return tris


def write_binary(path, tris, zero_normals=False):
    with open(path, "wb") as f:
        f.write(b"binarny STL".ljust(80, b"\0"))
        f.write(struct.pack("<I", len(tris)))
        for a, b, c in tris:
            n = (0.0, 0.0, 0.0) if zero_normals else norm(cross(sub(b, a), sub(c, a)))
            f.write(struct.pack("<12fH", *n, *a, *b, *c, 0))


def write_ascii(path, tris, name="piramida"):
    with open(path, "w", newline="\n") as f:
        f.write("solid %s\n" % name)
        for a, b, c in tris:
            n = norm(cross(sub(b, a), sub(c, a)))
            f.write("  facet normal %g %g %g\n    outer loop\n" % n)
            for v in (a, b, c):
                f.write("      vertex %g %g %g\n" % v)
            f.write("    endloop\n  endfacet\n")
        f.write("endsolid %s\n" % name)


write_binary(os.path.join(OUT, "01-szescian.stl"), cube())
write_ascii(os.path.join(OUT, "02-piramida.stl"), pyramid())
write_binary(os.path.join(OUT, "03-zerowe-normalne.stl"), octahedron(), zero_normals=True)

# uszkodzony: naglowek binarny deklaruje 1000 trojkatow, a danych brak
with open(os.path.join(OUT, "04-uszkodzony.stl"), "wb") as f:
    f.write(b"uszkodzony".ljust(80, b"\0"))
    f.write(struct.pack("<I", 1000))
    f.write(b"\x01\x02\x03")

# pusty
open(os.path.join(OUT, "05-pusty.stl"), "wb").close()

print("zapisano w", OUT)
