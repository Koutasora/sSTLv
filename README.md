# sSTLv (Simple STL Viewer)

Lekka przeglądarka plików STL dla Windows (jak prosta przeglądarka obrazów). C# 5 + WinForms + OpenGL 1.1,
bez zewnętrznych bibliotek, exe ok. 50 KB (z ikoną). Rysuje tylko na zdarzenia (brak timerów i pętli), w spoczynku 0% CPU.

## Budowanie

    build.cmd

Wynik: `bin\sSTLv.exe` oraz `bin\sSTLv-instalator.exe` (zawiera osadzone sSTLv.exe). Wymaga tylko wbudowanego kompilatora .NET Framework (`csc.exe`).

## Pobieranie

Z GitHub Releases:

- `sSTLv-instalator.exe`: instaluje program dla bieżącego użytkownika (bez administratora). Odinstalowanie:
  Ustawienia > Aplikacje > Zainstalowane aplikacje > sSTLv (albo `sSTLv-instalator.exe /odinstaluj`).
- `sSTLv-portable.exe`: wersja przenośna, wystarczy uruchomić, nic nie instaluje ani nie zapisuje.

Pliki nie są podpisane, więc SmartScreen może ostrzec: "Więcej informacji" > "Uruchom mimo to".

## Instalacja ze źródeł

Bez uprawnień administratora, tylko dla bieżącego użytkownika (HKCU):

1. Dwukrotnie kliknij `instaluj.cmd` (w razie potrzeby sam zbuduje program). Kopiuje `sSTLv.exe` do
   `%LOCALAPPDATA%\Programs\sSTLv` i rejestruje obsługę .stl ("Otwórz za pomocą", Aplikacje domyślne).
2. Windows nie pozwala ustawić domyślnej aplikacji dla .stl po cichu: w otwartych Ustawieniach kliknij
   "Ustaw domyślne" dla sSTLv.

## Odinstalowanie

Dwukrotnie kliknij `odinstaluj.cmd` (albo użyj instalatora z Releases): usuwa wpisy w rejestrze dodane przez instalator oraz folder
`%LOCALAPPDATA%\Programs\sSTLv`. Wybór domyślnej aplikacji zapisany przez Windows (UserChoice) nie jest ruszany.

## Użycie

- `sSTLv.exe plik.stl` albo "Otwórz za pomocą"; można też przeciągnąć plik .stl na okno (drag & drop).
- Bez argumentu: puste okno.

## Klawisze

- Lewo / Prawo: poprzedni / następny plik .stl w folderze (sortowanie jak w Eksploratorze, zapętlone).
- `D`: przełącza widok 2D / 3D i za każdym razem resetuje widok (w obu kierunkach).
- `Esc`: zamyka okno.

Brak innych klawiszy.

## Wskaźnik trybu

Mały półprzezroczysty kwadrat w lewym górnym rogu pokazuje bieżący tryb: "2D" albo "3D" (w obu trybach wygląda tak samo, zmienia się tylko napis). Tylko wyświetla, nie reaguje na mysz.

W lewym dolnym rogu widoku jest półprzezroczysta podpowiedź "Press D to change view | Press ESC to quit" (klawisze D i ESC w ramkach). Znika po 10 sekundach od uruchomienia (raz na start programu) i chowa się, gdy okno jest za wąskie.

## Mysz

- Kółko: zoom do kursora (w 2D i w 3D).
- 2D (stały kąt izometryczny): przeciąganie dowolnym przyciskiem przesuwa widok.
- 3D (perspektywa): lewy przycisk obraca, prawy / środkowy przesuwa.
- Zmiana pliku resetuje widok (tryb 2D/3D zostaje).

## Motyw

Jasny / ciemny wybierany automatycznie z ustawień Windows: ciemny tylko gdy `AppsUseLightTheme` = 0, w pozostałych przypadkach jasny. Odczyt raz przy starcie.
W trybie ciemnym pasek tytułu też jest ciemny.

## Testy

`python test\generuj.py` zapisuje małe pliki testowe do `test\pliki\` (sześcian binarny, piramida ASCII,
zerowe normalne, plik uszkodzony, plik pusty).

## Ikona

`ikona\sSTLv.ico` jest generowana skryptem `powershell -ExecutionPolicy Bypass -File ikona\generuj_ikone.ps1` (System.Drawing, bez bibliotek) i wbudowywana w exe przez build.cmd.
