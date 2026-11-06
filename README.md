# stl-podglad

Lekka przeglądarka plików STL dla Windows (jak prosta przeglądarka obrazów). C# 5 + WinForms + OpenGL 1.1,
bez zewnętrznych bibliotek, exe ok. 20 KB. Rysuje tylko na zdarzenia (brak timerów i pętli), w spoczynku 0% CPU.

## Budowanie

    build.cmd

Wynik: `bin\stl-podglad.exe`. Wymaga tylko wbudowanego kompilatora .NET Framework (`csc.exe`).

## Użycie

- `stl-podglad.exe plik.stl` albo "Otwórz za pomocą"; można też przeciągnąć plik .stl na okno.
- Bez argumentu: puste okno.

## Klawisze

- Lewo / Prawo: poprzedni / następny plik .stl w folderze (sortowanie jak w Eksploratorze, zapętlone).
- `D`: przełącza widok 2D / 3D.
- `Esc`: zamyka okno.

Brak innych klawiszy.

## Mysz

- 2D (stały kąt izometryczny): kółko zoom do kursora, lewy przycisk przesuwa.
- 3D (perspektywa): lewy przycisk obraca, prawy / środkowy przesuwa, kółko zoom.
- Zmiana pliku resetuje widok (tryb 2D/3D zostaje).

## Motyw

Jasny / ciemny wybierany automatycznie z ustawień Windows (`AppsUseLightTheme`), odczyt raz przy starcie.
W trybie ciemnym pasek tytułu też jest ciemny.

## Testy

`python test\generuj.py` zapisuje małe pliki testowe do `test\pliki\` (sześcian binarny, piramida ASCII,
zerowe normalne, plik uszkodzony, plik pusty).
