# Checklista stl-podglad

## Sprawdzone kompilacją
- [x] Okno WinForms + OpenGL 1.1 (P/Invoke), bez zewnętrznych bibliotek
- [x] build.cmd (csc, C# 5), SetProcessDPIAware
- [x] Pliki testowe (test\generuj.py)

## Sprawdzone w działaniu (AMD Radeon Vega 11, Windows 11, ciemny motyw)
- [x] Rysowanie tylko na zdarzenia, 0% CPU w spoczynku (0 ms CPU w 10 s)
- [x] Niskie zużycie RAM: working set 7-15 MB, także z plikiem 1 mln trójkątów
- [x] Wczytywanie do bufora VBO; plik 50 MB (1 mln trójkątów) otwiera się w ok. 0,5 s
- [x] Binarny i ASCII STL, normalne liczone przy zerowych
- [x] Binarny plik z nagłówkiem "solid" i nadmiarowymi bajtami
- [x] Uszkodzony / pusty plik: tytuł "błąd odczytu", pusty widok, bez wyjątku
- [x] Plik z argumentu wiersza poleceń
- [x] Lewo / Prawo: poprzedni / następny .stl (naturalne sortowanie, zapętlenie), właściwy model po zmianie pliku
- [x] D: przełączanie 2D / 3D z resetem widoku
- [x] Esc zamyka okno
- [x] 2D: rzut ortogonalny, stały kąt
- [x] 3D: perspektywa, obrót lewym przyciskiem
- [x] Kółko: zoom, działa po najechaniu bez klikania
- [x] Reset widoku przy zmianie pliku
- [x] Ciemny motyw i ciemny pasek tytułu
- [x] Tytuł: nazwa, indeks/liczba, tryb; brak menu, pasków i napisów w oknie
- [x] Wygląd: światła, kontrast na ciemnym tle

## Do sprawdzenia w działaniu
- [ ] Jasny motyw (AppsUseLightTheme = 1)
- [ ] Drag & drop pliku na okno
- [ ] Plik nie-.stl z argumentu
- [ ] ASCII z BOM / białymi znakami przed "solid"
- [ ] Przesuwanie w 2D i 3D (prawy / środkowy przycisk)
- [ ] Auto-repeat D ignorowany
