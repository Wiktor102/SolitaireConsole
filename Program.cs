using System.Text;

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
				if (!hardMode.HasValue) {
					playAgain = false;
					continue;
				}

				Game game = new(hardMode.Value); // Rozpocznij nową grę

				// Uruchom główną pętlę gry (teraz w klasie Game)
				Game.GameResult result = game.RunGameLoop();

				// Obsłuż wynik gry
				switch (result) {
					case Game.GameResult.Restart:
						// Będziemy kontynuować zewnętrzną pętlę, aby rozpocząć nową grę
						Console.WriteLine("\nRozpoczynanie nowej gry...");
						Thread.Sleep(1000); // Krótka pauza
						break;
					case Game.GameResult.Quit:
						playAgain = false; // Zakończ zewnętrzną pętlę
						break;
					case Game.GameResult.Continue:
					default:
						// Po normalnej grze (np. po wygranej) również kontynuujemy
						break;
				}
			}

			Console.WriteLine("\nDziękujemy za grę! Do zobaczenia!");
		}

		// Metoda do wyboru poziomu trudności z nawigacją strzałkami
		static bool? ChooseDifficulty() {
			string[] options = ["Łatwy (dobieranie 1 karty)", "Trudny (dobieranie 3 kart)", "Wyjdź"];
			int selectedIndex = 0;

			ConsoleKeyInfo key;
			do {
				Console.Clear();
				Console.WriteLine("Wybierz poziom trudności (użyj strzałek góra/dół i Enter):");
				Console.WriteLine("---------------------------------------------------------");

				for (int i = 0; i < options.Length; i++) {
					if (i == selectedIndex) {
						Console.BackgroundColor = ConsoleColor.Gray;
						Console.ForegroundColor = ConsoleColor.Black;
						Console.Write("> "); // Wskaźnik wybranej opcji
					} else {
						Console.Write("  ");
					}
					Console.WriteLine(options[i]);
					Console.ResetColor(); // Resetuj kolory po każdej opcji
				}
				Console.WriteLine("---------------------------------------------------------");

				key = Console.ReadKey(true); // true - nie wyświetlaj wciśniętego klawisza

				switch (key.Key) {
					case ConsoleKey.UpArrow:
						selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
						break;
					case ConsoleKey.DownArrow:
						selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
						break;
					case ConsoleKey.Escape: // Dodatkowa opcja wyjścia przez Escape
						selectedIndex = options.Length - 1; // Ustaw na "Wyjdź"
						goto case ConsoleKey.Enter; // Przejdź do logiki Enter
					case ConsoleKey.Enter:
						Console.Clear(); // Wyczyść menu przed wyświetleniem komunikatu
						switch (selectedIndex) {
							case 0:
								Console.WriteLine("Wybrano poziom łatwy.");
								Thread.Sleep(500);
								return false; // false oznacza tryb łatwy
							case 1:
								Console.WriteLine("Wybrano poziom trudny.");
								Thread.Sleep(500);
								return true; // true oznacza tryb trudny
							case 2:
								Console.WriteLine("Wyjście z gry.");
								Thread.Sleep(500);
								return null; // Sygnał do wyjścia z gry
						}
						break;
				}
			} while (key.Key != ConsoleKey.Enter); // Pętla działa dopóki nie wciśnięto Enter

			return null; // Domyślny return na wszelki wypadek (nie powinien być osiągnięty)
		}
	}
}
