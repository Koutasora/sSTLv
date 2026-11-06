# Checklista sSTLv

## Sprawdzone kompilacją
- [x] Okno WinForms + OpenGL 1.1 (P/Invoke), bez zewnętrznych bibliotek
- [x] build.cmd (csc, C# 5), SetProcessDPIAware
- [x] Pliki testowe (test\generuj.py)
- [x] Nazwa sSTLv, wersja 1.0.0.0 (AssemblyInfo.cs)
- [x] Instalator sSTLv-instalator.exe (osadzone sSTLv.exe, manifest asInvoker, DPI)
- [x] Skrypty instaluj / odinstaluj napisane (składnia sprawdzona parserem PowerShell)

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
- [x] Ikona programu w exe i na pasku tytułu okna, generator ikona/generuj_ikone.ps1 (napis sSTLv, w 16-32 px litera s)
- [ ] Ikona na plikach .stl w Eksploratorze po instalacji
- [x] Tytuł: nazwa, indeks/liczba, tryb; brak menu, pasków i napisów w oknie
- [x] Wygląd: światła, kontrast na ciemnym tle

## Do sprawdzenia w działaniu
- [x] Podpowiedź w lewym dolnym rogu, D i ESC pogrubioną kursywą (LAB)
- [ ] Podpowiedź chowa się w wąskim oknie
- [x] Podpowiedź znika po 10 s, potem 0 ms CPU w 10 s (LAB)
- [x] Wskaźnik trybu 2D/3D w lewym górnym rogu, ten sam kolor w obu trybach (LAB)
- [ ] Wskaźnik: puste okno, inne skalowanie DPI
- [x] sSTLv-instalator.exe: instalacja bez administratora, wpisy HKCU, wpis w Aplikacjach zainstalowanych, okno końcowe (LAB)
- [x] Instalator: ponowna instalacja na istniejącej (LAB)
- [ ] Instalator: odmowa gdy sSTLv działa
- [x] `/odinstaluj` (polecenie z wpisu Aplikacji zainstalowanych): usuwa wpisy, exe i folder razem z instalatorem (LAB)
- [ ] Odinstalowanie kliknięte w Ustawieniach
- [ ] Wersja przenośna sSTLv.exe nie zapisuje nic poza plikiem
- [x] instaluj.cmd: kopia do %LOCALAPPDATA%, wpisy HKCU, wersja w komunikacie, strona sSTLv w Aplikacjach domyślnych (sprawdzone na LAB)
- [x] odinstaluj.cmd usuwa tylko własne wpisy i folder (sprawdzone na LAB)
- [ ] Jasny motyw (AppsUseLightTheme = 1)
- [ ] Drag & drop pliku na okno
- [ ] Plik nie-.stl z argumentu
- [ ] ASCII z BOM / białymi znakami przed "solid"
- [ ] Przesuwanie w 2D i 3D (prawy / środkowy przycisk)
- [ ] Auto-repeat D ignorowany
