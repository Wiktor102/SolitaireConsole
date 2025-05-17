using SolitaireConsole.UI;

namespace SolitaireConsole {
	/// <summary>
	/// Klasa odpowiedzialna za wyświetlanie menu wyboru poziomu trudności
	/// </summary>
	public class DifficultySelector {
		private readonly Menu<DifficultyLevel?> _menu = new(Menu<int>.GAME_TITLE_HEADING, ["Wybierz poziom trudności:"], [
				new("Łatwy (dobieranie 1 karty)", DifficultyLevel.Easy),
				new("Trudny (dobieranie 3 kart)", DifficultyLevel.Hard),
				new("Wyjdź", null)
			]
		);

		/// <summary>
		/// Wyświetla menu wyboru poziomu trudności i zwraca wybrany poziom
		/// </summary>
		/// <returns>DifficultyLevel.Easy, DifficultyLevel.Hard lub null dla wyjścia</returns>
		public DifficultyLevel? ChooseDifficulty() {
			return _menu.Select();
		}
	}
}