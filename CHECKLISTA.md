# Checklista stl-podglad

## Sprawdzone kompilacją
- [x] Okno WinForms + OpenGL 1.1 (P/Invoke), bez zewnętrznych bibliotek
- [x] build.cmd (csc, C# 5), SetProcessDPIAware
- [x] Pliki testowe (test\generuj.py)

## Do sprawdzenia w działaniu
- [ ] Rysowanie tylko na zdarzenia, 0% CPU w spoczynku
- [ ] Niskie zużycie RAM, przycięcie working set raz po pierwszym namalowaniu
- [ ] Wczytywanie do display listy z tablic wierzchołków (szybkie dla dużych plików)
- [ ] Binarny i ASCII STL (także z BOM i białymi znakami przed "solid"), normalne liczone przy zerowych
- [ ] Binarny plik z nagłówkiem "solid" i nadmiarowymi bajtami
- [ ] Uszkodzony / pusty plik: tytuł "błąd odczytu", pusty widok, bez wyjątku
- [ ] Plik z argumentu wiersza poleceń (także nie-.stl) i drag & drop
- [ ] Lewo / Prawo: poprzedni / następny .stl (naturalne sortowanie, zapętlenie)
- [ ] D: przełączanie 2D / 3D z resetem widoku w obu kierunkach; auto-repeat D ignorowany
- [ ] Esc zamyka okno; brak innych klawiszy
- [ ] 2D: rzut ortogonalny, stały kąt, przesuwanie dowolnym przyciskiem
- [ ] 3D: perspektywa, obrót lewym, przesuwanie prawym / środkowym
- [ ] Kółko: zoom do kursora w 2D i 3D, działa po najechaniu bez klikania
- [ ] Reset widoku przy zmianie pliku
- [ ] Motyw z ustawień Windows (ciemny tylko gdy AppsUseLightTheme = 0)
- [ ] Ciemny pasek tytułu w trybie ciemnym
- [ ] Tytuł: nazwa, indeks/liczba, tryb; brak menu, pasków i napisów w oknie
- [ ] Wygląd: światła, oświetlenie dwustronne, kontrast na obu tłach
