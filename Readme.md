# Pasjans Konsolowy (C#)

## Opis

Prosta implementacja klasycznej gry karcianej Pasjans, działająca w oknie konsoli. Celem gry jest przeniesienie wszystkich kart na cztery stosy końcowe (Fundacje), układając je według kolorów (kier ♥, karo ♦, pik ♠, trefl ♣) od Asa do Króla.

## Jak uruchomić projekt

1.  **Środowisko:** Upewnij się, że masz zainstalowane środowisko .NET SDK (zalecane .NET 6.0 lub nowsze). Możesz je pobrać ze strony [Microsoft .NET](https://dotnet.microsoft.com/download).
2.  **Kompilacja:**
    * Otwórz terminal (wiersz poleceń, PowerShell, itp.).
    * Przejdź do katalogu, w którym zapisałeś plik `.cs` (lub pliki, jeśli podzieliłeś kod na klasy).
    * Użyj komendy `dotnet build`, aby skompilować projekt. Powinien zostać utworzony plik wykonywalny (np. w podkatalogu `bin/Debug/netX.Y/`).
3.  **Uruchomienie:**
    * W tym samym terminalu, użyj komendy `dotnet run`, aby uruchomić grę bezpośrednio.
    * Alternatywnie, przejdź do katalogu z plikiem wykonywalnym (np. `bin/Debug/netX.Y/`) i uruchom go bezpośrednio (np. wpisując `nazwa_projektu.exe` w Windows lub `./nazwa_projektu` w Linux/macOS).

**Uwaga:** Upewnij się, że Twój terminal obsługuje poprawnie kodowanie UTF-8, aby symbole kart (♥ ♦ ♣ ♠) wyświetlały się prawidłowo. W Windows Terminal zazwyczaj działa to domyślnie. W starszych konsolach Windows (cmd.exe) może być konieczna zmiana strony kodowej (`chcp 65001`).

## Instrukcje rozgrywki

Gra rozpoczyna się od wyboru poziomu trudności:

* **Łatwy:** Dobierasz po jednej karcie ze stosu rezerwowego [S].
* **Trudny:** Dobierasz po trzy karty ze stosu rezerwowego [S], ale możesz użyć tylko wierzchniej z dobranych kart.

**Elementy gry:**

* **[S] Stos Rezerwowy (Stock):** Zawiera zakryte karty. Wpisz `d` lub `draw`, aby dobrać karty. Jeśli stos jest pusty, wpisanie `d` przeniesie karty ze stosu odrzuconych [W] z powrotem do [S] (po przetasowaniu).
* **[W] Stos Odrzuconych (Waste):** Tu trafiają karty dobrane ze stosu [S]. Możesz przenieść wierzchnią kartę z [W] na stosy końcowe [F] lub kolumny gry [T].
* **[F1-F4] Stosy Końcowe (Foundations):** Cztery stosy, na które musisz ułożyć karty. Każdy stos musi zaczynać się od Asa i kończyć na Królu tego samego koloru (np. A♥, 2♥, ..., K♥).
* **[T1-T7] Kolumny Gry (Tableaux):** Siedem kolumn z kartami. Możesz przenosić odkryte karty między kolumnami, układając je w porządku malejącym (K, Q, ..., A) i naprzemiennymi kolorami (czerwony-czarny-czerwony...). Możesz przenosić pojedyncze karty lub całe poprawne sekwencje odkrytych kart. Na pustą kolumnę możesz położyć tylko Króla (lub sekwencję zaczynającą się od Króla). Jeśli przeniesiesz ostatnią odkrytą kartę z kolumny, karta pod nią (jeśli jest) zostanie automatycznie odkryta.

**Komendy:**

* `d` lub `draw`: Dobierz karty ze stosu [S].
* `m [źr] [cel]` lub `move [źr] [cel]`: Przenieś wierzchnią kartę ze stosu `[źr]` na stos `[cel]`.
    * Przykłady:
        * `m W T2`: Przenieś kartę z Waste [W] na Tableau [T2].
        * `m T1 F1`: Przenieś kartę z Tableau [T1] na Foundation [F1].
        * `m T3 T5`: Przenieś wierzchnią kartę z Tableau [T3] na Tableau [T5].
        * `m F2 T4`: Przenieś kartę z Foundation [F2] na Tableau [T4].
* `m [źr] [cel] [liczba]` lub `move [źr] [cel] [liczba]`: Przenieś sekwencję `[liczba]` kart z kolumny `[źr]` (tylko T) na kolumnę `[cel]` (tylko T).
    * Przykład: `m T4 T1 3`: Przenieś 3 wierzchnie odkryte karty z Tableau [T4] na Tableau [T1].
* `u` lub `undo`: Cofnij ostatni wykonany ruch (można cofnąć do 3 ruchów).
* `h` lub `score`: Wyświetl ranking najlepszych wyników.
* `r` lub `restart`: Rozpocznij nową grę od początku.
* `q` lub `quit`: Zakończ grę.

**Zwycięstwo:** Gra kończy się wygraną, gdy wszystkie 52 karty znajdą się na stosach końcowych [F1-F4]. Twój wynik (liczba ruchów) zostanie zapisany w rankingu.

## Opis klas i metod (w kodzie)

Szczegółowy opis poszczególnych klas (`Card`, `Deck`, `StockPile`, `WastePile`, `FoundationPile`, `TableauPile`, `Game`, `MoveRecord`, `HighScoreManager`, `Program`) oraz ich metod znajduje się bezpośrednio w kodzie źródłowym w formie komentarzy w języku polskim. Komentarze wyjaśniają przeznaczenie każdej klasy, pola oraz logikę działania ważniejszych metod.
