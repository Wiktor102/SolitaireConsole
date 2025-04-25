using System.Text;

namespace SolitaireConsole {
	/// <summary>
	/// Klasa odpowiedzialna za wyświetlanie menu wyboru poziomu trudności
	/// </summary>
	public class DifficultySelector {
		private string[] options = ["Łatwy (dobieranie 1 karty)", "Trudny (dobieranie 3 kart)", "Wyjdź"];
		private int selectedIndex = 0;

		/// <summary>
		/// Wyświetla menu wyboru poziomu trudności i zwraca wybrany poziom
		/// </summary>
		/// <returns>DifficultyLevel.Easy, DifficultyLevel.Hard lub null dla wyjścia</returns>
		public DifficultyLevel? ChooseDifficulty() {
			ConsoleKeyInfo key;

			do {
				DisplayMenu();
				key = Console.ReadKey(true); // true - nie wyświetlaj wciśniętego klawisza
				HandleKeyInput(key);

				if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape) {
					return ProcessSelection();
				}
			} while (true);
		}

		/// <summary>
		/// Wyświetla menu wyboru poziomu trudności
		/// </summary>
		private void DisplayMenu() {
			Console.Clear();
			Console.WriteLine("Wybierz poziom trudności (użyj strzałek góra/dół i Enter):");
			Console.WriteLine("---------------------------------------------------------");

			for (int i = 0; i < options.Length; i++) {
				DisplayMenuItem(i);
			}

			Console.WriteLine("---------------------------------------------------------");
		}

		/// <summary>
		/// Wyświetla pojedynczą opcję menu
		/// </summary>
		private void DisplayMenuItem(int index) {
			if (index == selectedIndex) {
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.Write("> "); // Wskaźnik wybranej opcji
			} else {
				Console.Write("  ");
			}

			Console.WriteLine(options[index]);
			Console.ResetColor(); // Resetuj kolory po każdej opcji
		}

		/// <summary>
		/// Obsługuje naciśnięcie klawiszy (strzałki góra/dół)
		/// </summary>
		private void HandleKeyInput(ConsoleKeyInfo key) {
			switch (key.Key) {
				case ConsoleKey.UpArrow:
					UpdateSelectedIndex(-1);
					break;
				case ConsoleKey.DownArrow:
					UpdateSelectedIndex(1);
					break;
				case ConsoleKey.Escape: // Dodatkowa opcja wyjścia przez Escape
					selectedIndex = options.Length - 1; // Ustaw na "Wyjdź"
					break;
			}
		}

		/// <summary>
		/// Aktualizuje indeks wybranej opcji z odpowiednią obsługą zawijania
		/// </summary>
		private void UpdateSelectedIndex(int direction) {
			if (direction < 0) {
				// Przesuń w górę (z zawijaniem)
				selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
			} else {
				// Przesuń w dół (z zawijaniem)
				selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
			}
		}

		/// <summary>
		/// Przetwarza wybraną opcję i zwraca odpowiednią wartość
		/// </summary>
		private DifficultyLevel? ProcessSelection() {
			Console.Clear(); // Wyczyść menu przed wyświetleniem komunikatu

			switch (selectedIndex) {
				case 0:
					Console.WriteLine("Wybrano poziom łatwy.");
					Thread.Sleep(500);
					return DifficultyLevel.Easy;
				case 1:
					Console.WriteLine("Wybrano poziom trudny.");
					Thread.Sleep(500);
					return DifficultyLevel.Hard;
				case 2:
					Console.WriteLine("Wyjście z gry.");
					Thread.Sleep(500);
					return null; // Sygnał do wyjścia z gry
				default:
					return null;
			}
		}
	}
}