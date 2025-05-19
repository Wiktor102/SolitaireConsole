using SolitaireConsole.UI;
using SolitaireConsole.Input; // Added for InputMode

namespace SolitaireConsole {
	/// <summary>
	/// Klasa odpowiedzialna za wyświetlanie menu wyboru poziomu trudności
	/// </summary>
	public class DifficultySelector {
		private readonly Menu<DifficultyLevel?> _difficultyMenu;
        private readonly Menu<int> _mainMenu; // Using int as a placeholder for action, could be an enum too
        private readonly SettingsMenu _settingsMenu;
        private readonly GameSettings _gameSettings;

		public DifficultySelector(GameSettings gameSettings) {
            _gameSettings = gameSettings;
			_difficultyMenu = new(Menu<int>.GAME_TITLE_HEADING, ["Wybierz poziom trudności:"], [
				new("Łatwy (dobieranie 1 karty)", DifficultyLevel.Easy),
				new("Trudny (dobieranie 3 kart)", DifficultyLevel.Hard),
				new("Wróć", null) // Changed from Wyjdź to Wróć
			]);

            _mainMenu = new(Menu<int>.GAME_TITLE_HEADING, ["Menu Główne:"], [
                new("Rozpocznij Grę", 1),
                new("Ustawienia", 2),
                new("Wyjdź", 0)
            ]);

            _settingsMenu = new SettingsMenu(_gameSettings);
		}

		/// <summary>
		/// Wyświetla menu wyboru poziomu trudności i zwraca wybrany poziom
		/// </summary>
		/// <returns>DifficultyLevel.Easy, DifficultyLevel.Hard lub null dla wyjścia</returns>
		public DifficultyLevel? ChooseDifficulty() {
			return _difficultyMenu.Select();
		}

        public MainMenuAction ShowMainMenu() {
            while (true) {
                int choice = _mainMenu.Select();
                switch (choice) {
                    case 1: // Rozpocznij Grę
                        return MainMenuAction.StartGame;
                    case 2: // Ustawienia
                        _settingsMenu.Show();
                        break; // Wróć do menu głównego po zamknięciu ustawień
                    case 0: // Wyjdź
                        return MainMenuAction.Exit;
                }
            }
        }
	}

    public enum MainMenuAction {
        StartGame,
        Exit
    }
}