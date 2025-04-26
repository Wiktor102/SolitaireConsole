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
							Console.WriteLine("Nie można dobrać karty.");
							game.Pause();
						}
						break;

					case "move":
					case "m":
						if (parts.Length < 3) {
							Console.WriteLine("Nieprawidłowa komenda 'move'. Użycie: move [źródło] [cel] [liczba_kart - opcjonalnie]");
							Console.WriteLine("Źródła: S (Stock - nie można), W (Waste), F1-F4 (Foundation), T1-T7 (Tableau)");
							Console.WriteLine("Cele: F1-F4, T1-T7");
							game.Pause();
							return;
						}

						string sourceStr = parts[1].ToUpper();
						string destStr = parts[2].ToUpper();
						int cardCount = 1; // Domyślnie przenosimy 1 kartę

						// Sprawdź, czy podano liczbę kart (dla ruchu T->T)
						if (parts.Length > 3) {
							if (!int.TryParse(parts[3], out cardCount) || cardCount < 1) {
								Console.WriteLine("Nieprawidłowa liczba kart. Musi być dodatnią liczbą całkowitą.");
								game.Pause();
								return;
							}
						}

						// Parsowanie źródła
						PileType sourceType;
						int sourceIndex = game.ParsePileString(sourceStr, out sourceType);
						if (sourceIndex == -1 || sourceType == PileType.Stock) // Nie można ruszać ze Stock bezpośrednio
						{
							Console.WriteLine($"Nieprawidłowe źródło: {sourceStr}");
							game.Pause();
							return;
						}

						// Parsowanie celu
						PileType destType;
						int destIndex = game.ParsePileString(destStr, out destType);
						if (destIndex == -1 || destType == PileType.Stock || destType == PileType.Waste) // Nie można ruszać na Stock ani Waste
						{
							Console.WriteLine($"Nieprawidłowy cel: {destStr}");
							game.Pause();
							return;
						}

						// Wykonaj ruch
						if (!game.TryMove(sourceType, sourceIndex, destType, destIndex, cardCount)) {
							// Komunikat o błędzie jest już wyświetlany w TryMove
							game.Pause();
						}
						break;

					case "undo":
					case "u":
						if (!game.UndoLastMove()) {
							// Komunikat o błędzie jest już wyświetlany w UndoLastMove
							game.Pause();
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
						game.Pause();
						break;
				}
			} catch (Exception ex) {
				Console.WriteLine($"\nWystąpił nieoczekiwany błąd: {ex.Message}");
				Console.WriteLine("Spróbuj ponownie lub uruchom grę od nowa.");
				game.Pause();
			}
		}
	}
}
