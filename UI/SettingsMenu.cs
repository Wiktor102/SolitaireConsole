using SolitaireConsole.Input;

namespace SolitaireConsole.UI {
    /// <summary>
    /// Klasa odpowiedzialna za wyświetlanie menu ustawień
    /// </summary>
    public class SettingsMenu {
        private readonly Menu<InputMode?> _menu;
        private readonly GameSettings _gameSettings;

        public SettingsMenu(GameSettings gameSettings) {
            _gameSettings = gameSettings;
            _menu = new Menu<InputMode?>(Menu<int>.GAME_TITLE_HEADING, ["Wybierz tryb sterowania:"], [
                new("Tryb tekstowy", InputMode.Text),
                new("Tryb strzałkowy", InputMode.Arrow),
                new("Wyjdź", null)
            ]);
        }

        /// <summary>
        /// Wyświetla menu ustawień i aktualizuje wybrane ustawienie
        /// </summary>
        public void Show() {
            InputMode? selectedMode = _menu.Select();
            if (selectedMode.HasValue) {
                _gameSettings.CurrentInputMode = selectedMode.Value;
            }
        }
    }

    /// <summary>
    /// Klasa przechowująca ustawienia gry
    /// </summary>
    public class GameSettings {
        public InputMode CurrentInputMode { get; set; } = InputMode.Arrow;
    }
}
