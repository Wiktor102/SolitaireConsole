using System;
using System.Text;
using System.Threading;

namespace SolitaireConsole {
    // Główna klasa programu
    class Program {
        static void Main(string[] args) {
            Console.Title = "Pasjans Konsolowy"; // Ustawia tytuł okna konsoli
            Console.OutputEncoding = Encoding.UTF8; // Ważne dla polskich znaków i symboli kart

            bool playAgain = true;
            while (playAgain) {
                // Wybór poziomu trudności
                bool? hardMode = ChooseDifficulty();
                if (!hardMode.HasValue) // Użytkownik wybrał wyjście
                {
                    playAgain = false;
                    continue;
                }

                Game game = new Game(hardMode.Value); // Rozpocznij nową grę

                // Główna pętla gry
                while (true) {
                    game.DisplayGame(); // Wyświetl stan gry

                    // Sprawdź warunek zwycięstwa
                    if (game.CheckWinCondition()) {
                        game.HandleWin(); // Obsłuż wygraną
                        break; // Zakończ pętlę gry
                    }

                    // Wyświetl dostępne akcje
                    Console.WriteLine("\nAkcje:");
                    Console.WriteLine(" - draw / d          : Dobierz kartę ze stosu [S]");
                    Console.WriteLine(" - move / m [źr] [cel]: Przenieś kartę/sekwencję (np. m W T2, m T1 F1, m T3 T5 [liczba])");
                    Console.WriteLine(" - undo / u          : Cofnij ostatni ruch (do 3 ruchów)");
                    Console.WriteLine(" - score / h         : Pokaż ranking");
                    Console.WriteLine(" - restart / r       : Rozpocznij nową grę");
                    Console.WriteLine(" - quit / q          : Zakończ grę");
                    Console.Write("Wybierz akcję: ");

                    string? input = Console.ReadLine()?.ToLower().Trim(); // Wczytaj i przetwórz komendę użytkownika

                    if (string.IsNullOrWhiteSpace(input)) continue; // Ignoruj puste linie

                    string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Podziel komendę na części
                    string command = parts[0];

                    try // Obsługa potencjalnych błędów parsowania komend
                    {
                        switch (command) {
                            case "draw":
                            case "d":
                                if (!game.DrawFromStock()) {
                                    Console.WriteLine("Nie można dobrać karty.");
                                    Pause();
                                }
                                break;

                            case "move":
                            case "m":
                                if (parts.Length < 3) {
                                    Console.WriteLine("Nieprawidłowa komenda 'move'. Użycie: move [źródło] [cel] [liczba_kart - opcjonalnie]");
                                    Console.WriteLine("Źródła: S (Stock - nie można), W (Waste), F1-F4 (Foundation), T1-T7 (Tableau)");
                                    Console.WriteLine("Cele: F1-F4, T1-T7");
                                    Pause();
                                    continue;
                                }
                                string sourceStr = parts[1].ToUpper();
                                string destStr = parts[2].ToUpper();
                                int cardCount = 1; // Domyślnie przenosimy 1 kartę

                                // Sprawdź, czy podano liczbę kart (dla ruchu T->T)
                                if (parts.Length > 3) {
                                    if (!int.TryParse(parts[3], out cardCount) || cardCount < 1) {
                                        Console.WriteLine("Nieprawidłowa liczba kart. Musi być dodatnią liczbą całkowitą.");
                                        Pause();
                                        continue;
                                    }
                                }

                                // Parsowanie źródła
                                PileType sourceType;
                                int sourceIndex = ParsePileString(sourceStr, out sourceType);
                                if (sourceIndex == -1 || sourceType == PileType.Stock) // Nie można ruszać ze Stock bezpośrednio
                                {
                                    Console.WriteLine($"Nieprawidłowe źródło: {sourceStr}"); Pause(); continue;
                                }

                                // Parsowanie celu
                                PileType destType;
                                int destIndex = ParsePileString(destStr, out destType);
                                if (destIndex == -1 || destType == PileType.Stock || destType == PileType.Waste) // Nie można ruszać na Stock ani Waste
                                {
                                    Console.WriteLine($"Nieprawidłowy cel: {destStr}"); Pause(); continue;
                                }

                                // Wykonaj ruch
                                if (!game.TryMove(sourceType, sourceIndex, destType, destIndex, cardCount)) {
                                    // Komunikat o błędzie jest już wyświetlany w TryMove
                                    Pause();
                                }
                                break;

                            case "undo":
                            case "u":
                                if (!game.UndoLastMove()) {
                                    // Komunikat o błędzie jest już wyświetlany w UndoLastMove
                                    Pause();
                                }
                                break;

                            case "score":
                            case "h":
                                Console.Clear();
                                game.DisplayHighScores();
                                Console.WriteLine("\nNaciśnij Enter, aby wrócić do gry...");
                                Console.ReadLine();
                                break;

                            case "restart":
                            case "r":
                                Console.Write("Czy na pewno chcesz rozpocząć nową grę? (t/n): ");
                                if (Console.ReadLine()?.ToLower() == "t") {
                                    goto NewGame; // Użycie goto do przeskoczenia do etykiety NewGame (poniżej pętli while)
                                }
                                break;

                            case "quit":
                            case "q":
                                Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
                                if (Console.ReadLine()?.ToLower() == "t") {
                                    playAgain = false; // Zakończ pętlę zewnętrzną (while playAgain)
                                    goto EndGame; // Użycie goto do wyjścia z pętli gry
                                }
                                break;

                            default:
                                Console.WriteLine("Nieznana komenda.");
                                Pause();
                                break;
                        }
                    } catch (Exception ex) {
                        Console.WriteLine($"\nWystąpił nieoczekiwany błąd: {ex.Message}");
                        Console.WriteLine("Spróbuj ponownie lub uruchom grę od nowa.");
                        Pause();
                    }
                } // Koniec pętli gry (while true)

            EndGame:; // Etykieta dla wyjścia z pętli gry

            NewGame: // Etykieta dla rozpoczęcia nowej gry
                if (playAgain) {
                    Console.WriteLine("\nRozpoczynanie nowej gry...");
                    System.Threading.Thread.Sleep(1000); // Krótka pauza
                }

            } // Koniec pętli ponownego grania (while playAgain)

            Console.WriteLine("\nDziękujemy za grę! Do zobaczenia!");
        }

        // Metoda do wyboru poziomu trudności
        static bool? ChooseDifficulty() {
            Console.Clear();
            Console.WriteLine("Wybierz poziom trudności:");
            Console.WriteLine(" 1. Łatwy (dobieranie 1 karty)");
            Console.WriteLine(" 2. Trudny (dobieranie 3 kart)");
            Console.WriteLine(" 3. Wyjdź");
            Console.Write("Wybór: ");

            while (true) {
                string? choice = Console.ReadLine();
                switch (choice) {
                    case "1":
                        Console.WriteLine("Wybrano poziom łatwy.");
                        Thread.Sleep(500);
                        return false; // false oznacza tryb łatwy
                    case "2":
                        Console.WriteLine("Wybrano poziom trudny.");
                        Thread.Sleep(500);
                        return true; // true oznacza tryb trudny
                    case "3":
                        return null; // Sygnał do wyjścia z gry
                    default:
                        Console.Write("Nieprawidłowy wybór. Wpisz 1, 2 lub 3: ");
                        break;
                }
            }
        }

        // Pomocnicza metoda do parsowania stringa reprezentującego stos (np. "T1", "F3", "W")
        // Zwraca indeks stosu (0-based) i ustawia typ stosu przez parametr 'out'
        // Zwraca -1 w przypadku błędu.
        static int ParsePileString(string pileStr, out PileType type) {
            type = PileType.Stock; // Domyślna wartość na wypadek błędu

            if (string.IsNullOrEmpty(pileStr)) return -1;

            char pileChar = pileStr[0];
            string indexStr = pileStr.Length > 1 ? pileStr.Substring(1) : "";
            int index = 0; // Domyślny indeks dla W

            switch (pileChar) {
                case 'W': // Waste Pile
                    if (pileStr.Length > 1) return -1; // Waste nie ma indeksu (W, nie W1)
                    type = PileType.Waste;
                    return 0; // Zwracamy 0 jako placeholder, bo Waste jest tylko jedno

                case 'F': // Foundation Pile
                    if (!int.TryParse(indexStr, out index) || index < 1 || index > 4) return -1; // Indeksy F1-F4
                    type = PileType.Foundation;
                    return index - 1; // Zwracamy indeks 0-based (0-3)

                case 'T': // Tableau Pile
                    if (!int.TryParse(indexStr, out index) || index < 1 || index > 7) return -1; // Indeksy T1-T7
                    type = PileType.Tableau;
                    return index - 1; // Zwracamy indeks 0-based (0-6)

                default:
                    return -1; // Nieznany typ stosu
            }
        }

        // Prosta metoda pauzująca grę do czasu naciśnięcia Enter
        static void Pause() {
            Console.WriteLine("\nNaciśnij Enter, aby kontynuować...");
            Console.ReadLine();
        }
    }
}
