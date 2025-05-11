using SolitaireConsole.CardPiles;

namespace SolitaireConsole.Input {
	public class TextInputStrategy(Game game) : InputStrategy(game) {
		public override void HandleInput(Action<GameResult> indicateGameEnd) {
			Console.Write("Wybierz akcję: ");
			string? input = Console.ReadLine()?.ToLower().Trim(); // Wczytaj i przetwórz komendę użytkownika
			if (string.IsNullOrWhiteSpace(input)) return; // Ignoruj puste linie

			string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Podziel komendę na części
			string command = parts[0];

			try { // Obsługa potencjalnych błędów parsowania komend
				switch (command) { // TODO: Rozdzielić na osobne metody dla kazdej komendy
					case "draw":
					case "d":
						if (!game.DrawFromStock()) {
							// TODO:
							// Error message for DrawFromStock is handled internally or via LastMoveError by DrawFromStock itself if necessary
							// No specific SetLastMoveError here unless DrawFromStock is changed to throw exceptions too.
						}
						break;

					case "move":
					case "m":
						if (parts.Length < 3) {
							game.SetLastMoveError("Nieprawidłowa komenda 'move'. Użycie: move [źródło] [cel] [liczba_kart - opcjonalnie]. Źródła: W, F1-F4, T1-T7. Cele: F1-F4, T1-T7.");
							return;
						}

						string sourceStr = parts[1].ToUpper();
						string destStr = parts[2].ToUpper();
						int cardCount = 1; // Domyślnie przenosimy 1 kartę

						// Sprawdź, czy podano liczbę kart (dla ruchu T->T)
						if (parts.Length > 3) {
							if (!int.TryParse(parts[3], out cardCount) || cardCount < 1) {
								game.SetLastMoveError("Nieprawidłowa liczba kart. Musi być dodatnią liczbą całkowitą.");
								return;
							}
						}

						// Parsowanie źródła
						PileType sourceType;
						int sourceIndex = ParsePileString(sourceStr, out sourceType);
						if (sourceIndex == -1 || sourceType == PileType.Stock) { // Nie można ruszać ze Stock bezpośrednio
							game.SetLastMoveError($"Nieprawidłowe źródło: {sourceStr}");
							return;
						}

						// Parsowanie celu
						PileType destType;
						int destIndex = ParsePileString(destStr, out destType);
						if (destIndex == -1 || destType == PileType.Stock || destType == PileType.Waste) { // Nie można ruszać na Stock ani Waste
							game.SetLastMoveError($"Nieprawidłowy cel: {destStr}");
							return;
						}

						// Wykonaj ruch
						game.TryMove(sourceType, sourceIndex, destType, destIndex, cardCount); // Błąd jest "łapany" i ustawiany wewnątrz metody Game.TryMove
						break;

					case "undo":
					case "u":
						if (!game.UndoLastMove()) game.Pause(); // Komunikat o błędzie jest już wyświetlany w UndoLastMove
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
							indicateGameEnd(GameResult.Restart); // Sygnalizuj chęć rozpoczęcia nowej gry
							return;
						}
						break;

					case "quit":
					case "q":
						Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
						if (Console.ReadLine()?.ToLower() == "t") {
							indicateGameEnd(GameResult.Quit); // Sygnalizuj chęć zakończenia gry
							return;
						}
						break;

					default:
						Console.WriteLine("Nieznana komenda.");
						game.Pause(); // Consider if this should set an error too.
						break;
				}
			} catch (Exception ex) {
				game.SetLastMoveError($"Wystąpił nieoczekiwany błąd: {ex.Message}");
			}
		}

		// Pomocnicza metoda do parsowania stringa reprezentującego stos (np. "T1", "F3", "W")
		// Zwraca indeks stosu (0-based) i ustawia typ stosu przez parametr 'out'
		// Zwraca -1 w przypadku błędu.
		private static int ParsePileString(string pileStr, out PileType type) {
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
	}
}
