using System.Text;

namespace SolitaireConsole {
	/// <summary>
	/// Klasa odpowiedzialna za wy�wietlanie menu wyboru poziomu trudno�ci
	/// </summary>
	public class DifficultySelector {
		private string[] options = ["�atwy (dobieranie 1 karty)", "Trudny (dobieranie 3 kart)", "Wyjd�"];
		private int selectedIndex = 0;

		/// <summary>
		/// Wy�wietla menu wyboru poziomu trudno�ci i zwraca wybrany poziom
		/// </summary>
		/// <returns>DifficultyLevel.Easy, DifficultyLevel.Hard lub null dla wyj�cia</returns>
		public DifficultyLevel? ChooseDifficulty() {
			ConsoleKeyInfo key;

			do {
				DisplayMenu();
				key = Console.ReadKey(true); // true - nie wy�wietlaj wci�ni�tego klawisza
				HandleKeyInput(key);

				if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.Escape) {
					return ProcessSelection();
				}
			} while (true);
		}

		/// <summary>
		/// Wy�wietla menu wyboru poziomu trudno�ci
		/// </summary>
		private void DisplayMenu() {
			Console.Clear();
			Console.WriteLine("Wybierz poziom trudno�ci (u�yj strza�ek g�ra/d� i Enter):");
			Console.WriteLine("---------------------------------------------------------");

			for (int i = 0; i < options.Length; i++) {
				DisplayMenuItem(i);
			}

			Console.WriteLine("---------------------------------------------------------");
		}

		/// <summary>
		/// Wy�wietla pojedyncz� opcj� menu
		/// </summary>
		private void DisplayMenuItem(int index) {
			if (index == selectedIndex) {
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.Write("> "); // Wska�nik wybranej opcji
			} else {
				Console.Write("  ");
			}

			Console.WriteLine(options[index]);
			Console.ResetColor(); // Resetuj kolory po ka�dej opcji
		}

		/// <summary>
		/// Obs�uguje naci�ni�cie klawiszy (strza�ki g�ra/d�)
		/// </summary>
		private void HandleKeyInput(ConsoleKeyInfo key) {
			switch (key.Key) {
				case ConsoleKey.UpArrow:
					UpdateSelectedIndex(-1);
					break;
				case ConsoleKey.DownArrow:
					UpdateSelectedIndex(1);
					break;
				case ConsoleKey.Escape: // Dodatkowa opcja wyj�cia przez Escape
					selectedIndex = options.Length - 1; // Ustaw na "Wyjd�"
					break;
			}
		}

		/// <summary>
		/// Aktualizuje indeks wybranej opcji z odpowiedni� obs�ug� zawijania
		/// </summary>
		private void UpdateSelectedIndex(int direction) {
			if (direction < 0) {
				// Przesu� w g�r� (z zawijaniem)
				selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
			} else {
				// Przesu� w d� (z zawijaniem)
				selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
			}
		}

		/// <summary>
		/// Przetwarza wybran� opcj� i zwraca odpowiedni� warto��
		/// </summary>
		private DifficultyLevel? ProcessSelection() {
			Console.Clear(); // Wyczy�� menu przed wy�wietleniem komunikatu

			switch (selectedIndex) {
				case 0:
					Console.WriteLine("Wybrano poziom �atwy.");
					Thread.Sleep(500);
					return DifficultyLevel.Easy;
				case 1:
					Console.WriteLine("Wybrano poziom trudny.");
					Thread.Sleep(500);
					return DifficultyLevel.Hard;
				case 2:
					Console.WriteLine("Wyj�cie z gry.");
					Thread.Sleep(500);
					return null; // Sygna� do wyj�cia z gry
				default:
					return null;
			}
		}
	}
}
