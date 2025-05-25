using SolitaireConsole.CardPiles;

namespace SolitaireConsole.Input {
	public class TextInputStrategy(Game game) : InputStrategy() {
		public Game Game { get; private set; } = game;

		public override void HandleInput(Action<GameResult> indicateGameEnd) {
			Console.Write("Wybierz akcję: ");
			string? input = Console.ReadLine()?.ToLower().Trim(); // Wczytaj i przetwórz komendę użytkownika
			if (string.IsNullOrWhiteSpace(input)) return; // Ignoruj puste linie

			string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Podziel komendę na części
			string command = parts[0];
			string[] args = parts.Length > 1 ? parts.Skip(1).ToArray() : [];

			try { // Obsługa potencjalnych błędów parsowania komend
				switch (command) {
					case "draw":
					case "d":
						HandleDrawCommand();
						break;

					case "move":
					case "m":
						HandleMoveCommand(args);
						break;

					case "undo":
					case "u":
						HandleUndoCommand();
						break;

					case "restart":
					case "r":
						HandleRestartCommand(indicateGameEnd);
						break;

					case "quit":
					case "q":
						HandleQuitCommand(indicateGameEnd);
						break;

					default:
						Console.WriteLine("Nieznana komenda.");
						Game.Pause();
						break;
				}
			} catch (Exception ex) {
				Game.SetLastMoveError($"Wystąpił nieoczekiwany błąd: {ex.Message}");
			}
		}

		/// <summary>
		/// Obsługuje komendę pobrania karty ze stosu.
		/// </summary>
		private void HandleDrawCommand() {
			if (!Game.DrawFromStock()) {
				// TODO:
				// Error message for DrawFromStock is handled internally or via LastMoveError by DrawFromStock itself if necessary
				// No specific SetLastMoveError here unless DrawFromStock is changed to throw exceptions too.
			}
		}

		/// <summary>
		/// Obsługuje komendę przeniesienia kart.
		/// </summary>
		/// <param name="args">Argumenty komendy.</param>
		private void HandleMoveCommand(string[] args) {
			if (args.Length < 1) {
				Game.SetLastMoveError("Nieprawidłowa komenda 'move'. Użycie: move [źródło] ([cel] [liczba_kart - opcjonalnie]). Źródła: W, F1-F4, T1-T7.");
				return;
			}

			string sourceStr = args[0].ToUpper();
			int sourceIndex = ParsePileString(sourceStr, out PileType sourceType);

			if (sourceIndex == -1 || sourceType == PileType.Stock) { // Nie można ruszać ze Stock bezpośrednio
				Game.SetLastMoveError($"Nieprawidłowe źródło: {sourceStr}");
				return;
			}

			// Sprawdź czy jest to próba automatycznego przeniesienia (podano tylko źródło)
			if (args.Length == 1) {
				if (!Game.GameSettings.AutoMoveToFoundation) {
					Game.SetLastMoveError("Automatyczne przenoszenie kart na fundament jest wyłączone. Musisz podać cel dla ruchu ręcznie. Możesz to zmienić w ustawieniach gry.");
					return; // Automatyczne przenoszenie jest wyłączone
				}

				if (sourceType != PileType.Tableau && sourceType != PileType.Waste) {
					Game.SetLastMoveError("Automatyczne przenoszenie jest możliwe tylko ze stosów Tableau (T) lub kart odrzuconych (W).");
					return;
				}

				if (Game.TryAutoMoveToFoundation(sourceType, sourceIndex)) return; // Ruch się powiódł

				// Wiadomość o błędzie jest ustawiana w TryAutoMoveToFoundation lub w TryMove
				// Jeśli zwróci false i nie ustawiono błędu, oznacza to, że nie znaleziono poprawnego automatycznego ruchu.
				if (string.IsNullOrEmpty(Game.LastMoveError)) {
					Game.SetLastMoveError("Nie można automatycznie przenieść karty na fundament.");
				}
				return;
			}

			// Zwykły ruch z źródła do celu
			if (args.Length < 2) {
				Game.SetLastMoveError("Nieprawidłowa komenda 'move'. Musisz podać cel dla tego typu ruchu.");
				return;
			}

			string destStr = args[1].ToUpper();
			int cardCount = 1; // Domyślnie przenosimy 1 kartę

			// Sprawdź, czy podano liczbę kart (dla ruchu T->T)
			if (args.Length > 2) {
				if (!int.TryParse(args[2], out cardCount) || cardCount < 1) {
					Game.SetLastMoveError("Nieprawidłowa liczba kart. Musi być dodatnią liczbą całkowitą.");
					return;
				}
			}

			// Parsowanie celu
			PileType destType;
			int destIndex = ParsePileString(destStr, out destType);
			if (destIndex == -1 || destType == PileType.Stock || destType == PileType.Waste) { // Nie można ruszać na Stock ani Waste
				Game.SetLastMoveError($"Nieprawidłowy cel: {destStr}");
				return;
			}

			// Wykonaj ruch
			Game.TryMove(sourceType, sourceIndex, destType, destIndex, cardCount); // Błąd jest "łapany" i ustawiany wewnątrz metody Game.TryMove
		}

		/// <summary>
		/// Obsługuje komendę cofnięcia ostatniego ruchu.
		/// </summary>
		private void HandleUndoCommand() {
			Game.UndoLastMove(); // Wiadomość o błędzie jest ustawiana w UndoLastMove i wyświetlana przez DisplayStrategy
		}

		/// <summary>
		/// Obsługuje komendę ponownego uruchomienia gry.
		/// </summary>
		/// <param name="indicateGameEnd">Akcja sygnalizująca zakończenie gry.</param>
		private void HandleRestartCommand(Action<GameResult> indicateGameEnd) {
			Console.Write("Czy na pewno chcesz rozpocząć nową grę? (t/n): ");
			if (Console.ReadLine()?.ToLower() == "t") {
				indicateGameEnd(GameResult.Restart); // Sygnalizuj chęć rozpoczęcia nowej gry
			}
		}

		/// <summary>
		/// Obsługuje komendę zakończenia gry.
		/// </summary>
		/// <param name="indicateGameEnd">Akcja sygnalizująca zakończenie gry.</param>
		private void HandleQuitCommand(Action<GameResult> indicateGameEnd) {
			Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
			if (Console.ReadLine()?.ToLower() == "t") {
				indicateGameEnd(GameResult.Quit); // Sygnalizuj chęć zakończenia gry
			}
		}

		// Pomocnicza metoda do parsowania stringa reprezentującego stos (np. "T1", "F3", "W")
		// Zwraca indeks stosu (0-based) i ustawia typ stosu przez parametr 'out'
		// Zwraca -1 w przypadku błędu.
		private static int ParsePileString(string pileStr, out PileType type) {
			type = PileType.Stock; // Domyślna wartość na wypadek błędu

			if (string.IsNullOrEmpty(pileStr)) return -1;

			// Pozwól na parsowanie S dla Stock dla potencjalnego przyszłego użycia, ale ruch z Stock jest nadal zabroniony przez logikę komendy.
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

		public override ConsoleKeyInfo ReadKey() {
			// TextInputStrategy nie używa bezpośredniego odczytu klawiszy do nawigacji po menu w taki sam sposób jak ArrowInputStrategy.
			// Ta metoda jest wykorzystywana głównie przez klasę Menu, gdy jest skonfigurowana do nawigacji strzałkami.
			// Jednak aby spełnić wymagania klasy abstrakcyjnej, możemy zwrócić domyślną wartość lub rzucić NotImplementedException,
			// jeśli bezpośredni odczyt klawiszy nie jest przewidziany dla tej strategii poza Console.ReadLine().
			// Na razie zwracamy domyślną wartość, ponieważ Menu może ją wywołać.
			return Console.ReadKey(true);
		}
	}
}
