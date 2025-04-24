using System.Text;

namespace SolitaireConsole {
	/// <summary>
	/// Klasa odpowiedzialna za wyœwietlanie menu wyboru poziomu trudnoœci
	/// </summary>
	public class DifficultySelector {
		private string[] options = ["£atwy (dobieranie 1 karty)", "Trudny (dobieranie 3 kart)", "WyjdŸ"];
		private int selectedIndex = 0;

		/// <summary>
		/// Wyœwietla menu wyboru poziomu trudnoœci i zwraca wybrany poziom
		/// </summary>
		/// <returns>DifficultyLevel.Easy, DifficultyLevel.Hard lub null dla wyjœcia</returns>
		public DifficultyLevel? ChooseDifficulty() {
			ConsoleKeyInfo key;

			do {
				DisplayMenu();
				key = Console.ReadKey(true); // true - nie wyœwietlaj wciœniêtego klawisza
				HandleKeyInput(key);

				if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape) {
					return ProcessSelection();
				}
			} while (true);
		}

		/// <summary>
		/// Wyœwietla menu wyboru poziomu trudnoœci
		/// </summary>
		private void DisplayMenu() {
			Console.Clear();
			Console.WriteLine("Wybierz poziom trudnoœci (u¿yj strza³ek góra/dó³ i Enter):");
			Console.WriteLine("---------------------------------------------------------");

			for (int i = 0; i < options.Length; i++) {
				DisplayMenuItem(i);
			}

			Console.WriteLine("---------------------------------------------------------");
		}

		/// <summary>
		/// Wyœwietla pojedyncz¹ opcjê menu
		/// </summary>
		private void DisplayMenuItem(int index) {
			if (index == selectedIndex) {
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.Write("> "); // WskaŸnik wybranej opcji
			} else {
				Console.Write("  ");
			}

			Console.WriteLine(options[index]);
			Console.ResetColor(); // Resetuj kolory po ka¿dej opcji
		}

		/// <summary>
		/// Obs³uguje naciœniêcie klawiszy (strza³ki góra/dó³)
		/// </summary>
		private void HandleKeyInput(ConsoleKeyInfo key) {
			switch (key.Key) {
				case ConsoleKey.UpArrow:
					UpdateSelectedIndex(-1);
					break;
				case ConsoleKey.DownArrow:
					UpdateSelectedIndex(1);
					break;
				case ConsoleKey.Escape: // Dodatkowa opcja wyjœcia przez Escape
					selectedIndex = options.Length - 1; // Ustaw na "WyjdŸ"
					break;
			}
		}

		/// <summary>
		/// Aktualizuje indeks wybranej opcji z odpowiedni¹ obs³ug¹ zawijania
		/// </summary>
		private void UpdateSelectedIndex(int direction) {
			if (direction < 0) {
				// Przesuñ w górê (z zawijaniem)
				selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
			} else {
				// Przesuñ w dó³ (z zawijaniem)
				selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
			}
		}

		/// <summary>
		/// Przetwarza wybran¹ opcjê i zwraca odpowiedni¹ wartoœæ
		/// </summary>
		private DifficultyLevel? ProcessSelection() {
			Console.Clear(); // Wyczyœæ menu przed wyœwietleniem komunikatu

			switch (selectedIndex) {
				case 0:
					Console.WriteLine("Wybrano poziom ³atwy.");
					Thread.Sleep(500);
					return DifficultyLevel.Easy;
				case 1:
					Console.WriteLine("Wybrano poziom trudny.");
					Thread.Sleep(500);
					return DifficultyLevel.Hard;
				case 2:
					Console.WriteLine("Wyjœcie z gry.");
					Thread.Sleep(500);
					return null; // Sygna³ do wyjœcia z gry
				default:
					return null;
			}
		}
	}
}
