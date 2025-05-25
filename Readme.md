# Pasjans Konsolowy (C#)

## Opis
**Gra została opracowana jako praca konkursowa w II etapie konkursu [Gigathon](https://www.gigathon.pl/) 2025**

Zaawansowana implementacja klasycznej gry karcianej Pasjans, działająca w oknie konsoli. Aplikacja oferuje przyjazny interfejs użytkownika z dwoma trybami sterowania, systemem ustawień, rankingiem wyników oraz obsługą cofania do 3 ostatnich ruchów. Celem gry jest przeniesienie wszystkich 52 kart na cztery stosy końcowe (Foundations), układając je według kolorów (kier ♥, karo ♦, pik ♠, trefl ♣) w kolejności od Asa do Króla.

## Jak uruchomić projekt
### 1. Uruchomienie już skompilowanego projektu
Gotowe kompilacje znajdują się w zakładce [Releases](https://github.com/Wiktor102/SolitaireConsole/releases). Wystarczy pobrać plik dla danego systemu operacyjnego:
#### Windows 11 (x64)
Wystarczy pobrać stamtąd plik `SolitaireConsole.exe` i uruchomic go na komputerze z systemem Windows 11.

#### Linux (x64)
By uruchomić na komputerze z systemem linux pobierz plik `SolitaireConsole-linux-x64`. Możesz musieć dodać uprawnienia do uruchamiania za pomocą komendy: `sudo chmod +x ./SolitaireConsole-linux-x64`. Potem możesz po prostu wykonać ten plik w wybranym terminalu.

### MacOS
Gra powinna też działać na sprzętach z systemem MacOS, lecz nie była na nich testowana. Wymagana jest samodzielna kompilacja

### 2. Samodzielna kompilacja
#### Wymagania systemowe:
- **.NET 8.0 SDK** lub nowszy - można pobrać ze strony [Microsoft .NET](https://dotnet.microsoft.com/download)
- Terminal obsługujący kodowanie UTF-8 (Windows Terminal, PowerShell 7+, lub nowoczesny terminal Linux/macOS)

1. **Klonowanie i przejście do katalogu:**
   ```bash
   git clone https://github.com/Wiktor102/SolitaireConsole.git
   cd SolitaireConsole
   ```

2. **Kompilacja projektu:**
   ```bash
   dotnet build
   ```

3. **Uruchomienie gry:**
   ```bash
   dotnet run
   ```

4. **Kompilacja wersji Release (na system windows, opcjonalnie):**
   ```bash
   dotnet publish -c Release --self-contained false -r win-x64
   ```

### Uwagi techniczne
- Aplikacja automatycznie ustawia kodowanie UTF-8 dla poprawnego wyświetlania symboli kart (♥ ♦ ♣ ♠)
- Ustawienia gry i ranking wyników są zapisywane w katalogu aplikacji (w tym samym co uruchamiany plik .exe)
- Do poprawnego wyświetlania symboli kart wymagane jest środowisko konsolowe obsługujące kodowanie znaków UTF-8. Zalecam użycie programu "Terminal" na systemie Windows 11. Dodatkowo, dla najlepszych wrażeń polecam ustwić czcionkę konsoli na "Cascadia Mono NF" (we wspomnianym programie "Terminal" można to ustawić przechodząc do ustawień, wybierając Open JSON file i ustawić atrybuty `profiles.defaults.font.face` oraz `profiles.defaults.font.weight`).

## Funkcje gry

### Menu główne i nawigacja
- **Menu główne** z opcjami: Rozpocznij Grę, Ustawienia, Wyjdź
- **Menu ustawień** pozwalające na konfigurację trybu sterowania i automatycznego przenoszenia kart
- **Wybór poziomu trudności** przed rozpoczęciem każdej gry
- **System rankingu** z zapisywaniem najlepszych wyników
- **Ekran zwycięstwa** z możliwością zapisania wyniku

### Poziomy trudności
- **Łatwy:** Dobieranie po jednej karcie ze stosu rezerwowego
- **Trudny:** Dobieranie po trzy karty ze stosu rezerwowego, z możliwością użycia tylko wierzchniej karty

### Tryby sterowania

#### 1. Tryb strzałkowy (Arrow Mode) - domyślny
Intuicyjne sterowanie za pomocą klawiszy strzałek i funkcyjnych:

**Nawigacja:**
- `↑↓ ←→` - poruszanie się po planszy gry
- `Enter` - wybór karty / potwierdzenie akcji
- `Ctrl + Enter` - przeniesienie na stos końcowy (jeśli możliwe)
- `Escape` - anulowanie wyboru celu

**Akcje specjalne:**
- `U` - cofnij ostatni ruch
- `Q` - wyjście z gry (z potwierdzeniem)

**Obszary nawigacji:**
- **Stos rezerwowy** - dobieranie kart
- **Stos odrzuconych** - karty dobrane ze stosu S
- **Stosy końcowe** - cele końcowe gry
- **Kolumny główne** - obszar układania kart

#### 2. Tryb tekstowy (Text Mode)
Klasyczne sterowanie przez wpisywanie komend tekstowych:

**Komendy podstawowe:**
- `d` / `draw` - dobierz karty ze stosu [S]
- `m [źródło] [cel]` / `move [źródło] [cel]` - przenieś kartę
- `m [źródło] [cel] [liczba]` - przenieś sekwencję kart (tylko T→T)
- `u` / `undo` - cofnij ostatni ruch
- `r` / `restart` - rozpocznij nową grę
- `q` / `quit` - zakończ grę

**Oznaczenia stosów:**
- `S` - Stos rezerwowy (Stock)
- `W` - Stos odrzuconych (Waste)  
- `F1-F4` - Stosy końcowe (Foundations)
- `T1-T7` - Kolumny główne (Tableaux)

**Przykłady komend:**
- `m W T2` - przenieś kartę z Waste na kolumnę T2
- `m T1 F1` - przenieś kartę z kolumny T1 na stos końcowy F1
- `m T3 T5 3` - przenieś 3 karty z kolumny T3 na T5

### Elementy planszy gry

**Stos Rezerwowy (Stock):**
- Zawiera zakryte karty do dobierania
- Dobieranie przez kliknięcie w trybie strzałkowym lub komendę `d`/`draw`
- Automatyczne przeniesienie kart z Waste po wyczerpaniu

**Stos Odrzuconych (Waste):**
- Karty dobrane ze stosu rezerwowego
- W trybie łatwym: widoczna 1 karta
- W trybie trudnym: widoczne 3 karty, zargać można tylko wierzchnią (karta wierzchnia ukazywana jest w normalnym kolorze po prawej stronie, natomiast karty niegrywalne są zawsze jasnoszare)

**Stosy Końcowe (Foundations):**
- Cele końcowe gry - od Asa do Króla tego samego koloru (kolejność: A, 2, 3, ..., 10, J, Q, K)
- Każdy stos końcowy ma od początku gry przypisany swój kolor
- Możliwość cofania kart w razie potrzeby

**Kolumny Główne (Tableaux):**
- Siedem kolumn do układania kart
- Reguła: naprzemienne kolory, malejące wartości (K, Q, J, 10, ..., 2, A)
- Automatyczne odkrywanie kart po przeniesieniu
- Na pustą kolumnę można położyć tylko Króla lub całą sekwencję kart rozpoczynającą się od króla

### Zaawansowane funkcje

**System cofania (Undo):**
- Cofanie do 3 ostatnich ruchów
- Pełne przywracanie stanu planszy
- Obsługa wszystkich typów ruchów włącznie z dobieraniem

**Automatyczne przenoszenie:**
- Opcjonalne automatyczne przenoszenie kart na stosy końcowe
- Konfigurowane w ustawieniach gry
- Oszczędza czas przy oczywistych ruchach

**System rankingu:**
- Automatyczne zapisywanie najlepszych wyników
- Sortowanie według liczby ruchów (mniej = lepiej)
- Wprowadzanie 3-znakowego nicku po wygranej
- Trwałe przechowywanie w pliku `highscores.txt`

**Zarządzanie ustawieniami:**
- Wybór trybu sterowania (strzałkowy/tekstowy)
- Włączanie/wyłączanie automatycznego przenoszenia
- Automatyczny zapis ustawień w pliku `settings.json`

## Zasady gry

**Cel gry:** Przenieś wszystkie 52 karty na cztery stosy końcowe, układając je według kolorów od Asa do Króla.

**Reguły przenoszenia:**
1. **Na stosy końcowe (Foundations):** Tylko karty tego samego koloru w kolejności A, 2, 3, ..., 10, J, Q, K
2. **Między kolumnami (Tableaux):** Naprzemienne kolory, malejące wartości (K, Q, J, 10, ..., 2, A)
3. **Na pustą kolumnę:** Tylko Król (lub sekwencja zaczynająca się od Króla)
4. **Sekwencje kart:** Można przenosić tylko prawidłowe, ciągłe sekwencje odkrytych kart

**Zwycięstwo:** Gra kończy się wygraną, gdy wszystkie karty znajdą się na stosach końcowych. Wynik (liczba ruchów) zostaje zapisany w rankingu po wprowadzeniu nicku.

## Architektura projektu

### Struktura katalogów

```
SolitaireConsole/
├── src/
│   ├── CardPiles/           # Implementacje różnych typów stosów kart
│   │   ├── CardPile.cs      # Klasa bazowa dla wszystkich stosów
│   │   ├── StockPile.cs     # Stos rezerwowy [S]
│   │   ├── WastePile.cs     # Stos odrzuconych [W]
│   │   ├── FoundationPile.cs # Stosy końcowe [F1-F4]
│   │   └── TableauPile.cs   # Kolumny główne [T1-T7]
│   ├── Input/               # System obsługi wejścia użytkownika
│   │   ├── InputStrategy.cs # Interfejs strategii wejścia
│   │   ├── ArrowInputStrategy.cs  # Obsługa trybu strzałkowego
│   │   ├── TextInputStrategy.cs   # Obsługa trybu tekstowego
│   │   └── InputMode.cs     # Enum trybów sterowania
│   ├── InteractionModes/    # Tryby interakcji z grą
│   │   ├── InteractionMode.cs     # Klasa bazowa trybów
│   │   ├── ArrowInteractionMode.cs # Logika trybu strzałkowego
│   │   └── TextInteractionMode.cs  # Logika trybu tekstowego
│   ├── UI/                  # Komponenty interfejsu użytkownika
│   │   ├── DisplayStrategy.cs     # System wyświetlania planszy
│   │   ├── Menu.cs          # System menu z renderowaniem
│   │   ├── SettingsMenu.cs  # Menu ustawień gry
│   │   ├── DifficultySelector.cs  # Wybór poziomu trudności
│   │   ├── WinScreen.cs     # Ekran zwycięstwa
│   │   ├── HighScoreScreen.cs     # Wyświetlanie rankingu
│   │   └── SettingsManager.cs     # Zarządzanie ustawieniami
│   ├── Utils/               # Klasy pomocnicze
│   ├── Game.cs              # Główna klasa gry
│   ├── MoveService.cs       # Serwis obsługi ruchów
│   ├── HighScoreManager.cs  # Zarządzanie rankingiem
│   ├── Card.cs              # Model karty (Card, Rank, Suit)
│   ├── Deck.cs              # Talia kart
│   ├── Program.cs           # Punkt wejścia aplikacji
│   └── SolitaireConsole.csproj
├── SolitaireConsole.sln
├── .editorconfig
├── .gitignore
├── Licence
└── Readme.md
```

### Główne komponenty systemu

#### 1. **System stosów kart (CardPiles)**
- **CardPile** - abstrakcyjna klasa bazowa definiująca wspólne operacje dla wszystkich stosów
- **PileType** - enum identyfikujący typy stosów (Stock, Waste, Foundation, Tableau)
- Każdy typ stosu implementuje własną logikę sprawdzania poprawności ruchów (`CanAddCard`)
- Wszechstronne wsparcie dla operacji dodawania/usuwania kart z automatycznym odkrywaniem

#### 2. **Wzorzec Strategy dla trybów sterowania**
- **InputStrategy** - interfejs definiujący strategię obsługi wejścia
- **ArrowInputStrategy** - implementacja dla trybu strzałkowego z nawigacją klawiaturą
- **TextInputStrategy** - implementacja dla trybu tekstowego z komendami
- **InteractionMode** - warstwa abstrakcji łącząca strategię z logiką gry

#### 3. **System interfejsu użytkownika**
- **DisplayStrategy** - abstrakcyjna klasa obsługi wyświetlania z metodami virtualnymi
- **ConsoleDisplayStrategy** - konkretna implementacja dla środowiska konsolowego
- **PileDisplayInfo** - struktura definiująca sposób wyświetlania stosów
- Modularny system menu z **MenuRenderer** umożliwiający łatwą zmianę stylu wyświetlania

#### 4. **Architektura gry (Game)**
- Centralna klasa **Game** zarządzająca stanem gry i logiką biznesową
- **MoveService** - dedykowany serwis obsługi ruchów z walidacją i historią
- **MoveRecord** - model reprezentujący pojedynczy ruch dla systemu cofania
- Wsparcie dla maksymalnie 3 cofnięć z pełnym przywracaniem stanu

#### 5. **System ustawień i persystencji**
- **GameSettings** - klasa przechowująca konfigurację gry z systemem powiadomień
- **SettingsManager** - obsługa zapisu/odczytu ustawień w formacie JSON
- **HighScoreManager** - zarządzanie rankingiem z persystencją w pliku tekstowym
- Automatyczne zapisywanie zmian ustawień z obsługą błędów I/O

### Wzorce projektowe

1. **Strategy Pattern** - dla trybów sterowania (Arrow/Text)
2. **Template Method** - w klasach bazowych CardPile i DisplayStrategy
3. **Factory Method** - w tworzeniu stosów o odpowiednich typach
4. **Observer** - w systemie powiadomień o zmianach ustawień
5. **Command** - w systemie cofania ruchów (MoveRecord)

### Kluczowe wyliczenia (enumeracje)

```csharp
public enum Suit { Hearts, Diamonds, Clubs, Spades }       // Kolory kart
public enum Rank { Ace=1, Two, ..., King }                 // Figury kart  
public enum CardColor { Red, Black }                       // Kolory (czerwony/czarny)
public enum PileType { Stock, Waste, Foundation, Tableau } // Typy stosów
public enum InputMode { Arrow, Text }                      // Tryby sterowania
public enum DifficultyLevel { Easy, Hard }                 // Poziomy trudności
```

### Technologie i wymagania

- **.NET 8.0**
- **System.Text.Json** - do serializacji ustawień użyte są wbudowane funkcje frameworka .NET
- **Kodowanie UTF-8** - symbole kart wyświetlane są jako znaki Unicode (♥ ♦ ♣ ♠)
- **Console API** - obsługa kolorów
- **Cross-platform** - kompatybilność z Windows, Linux i macOS

### Cechy architektury

- **Modularność** - klarowny podział odpowiedzialności między komponenty
- **Rozszerzalność** - łatwe dodawanie nowych trybów sterowania lub wyświetlania  
- **Testowalność** - separacja logiki biznesowej od interfejsu użytkownika
- **Wydajność** - optymalne zarządzanie pamięcią i minimalizacja alokacji
- **Łatwość utrzymania** - przejrzysty kod z konsekwentną konwencją nazewnictwa

## Licencja
Kod gry oraz skompilowaną grę (we wszystkich wersjach) obejmuje licencja _cc by-nc-nd 4.0_. Pełna treść dostępna jest w pliku [Licence](Licence). Dodatkowo udzielam jury konkursu [Gigathon](https://www.gigathon.pl/) wszelkich praw koniecznych do oceny pracy konkursowej (projektu gry zawartego w tym repozytorium oraz skompilowanych plików wykonywalnych).
