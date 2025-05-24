using SolitaireConsole.Input;

namespace SolitaireConsole.UI {
    /// <summary>
    /// Klasa odpowiedzialna za wyświetlanie menu ustawień
    /// </summary>
    public class SettingsMenu {
        private readonly GameSettings _gameSettings;
        private readonly SettingsItem[] _settingsItems;
        private readonly MenuRenderer _menuRenderer = new ConsoleMenuRenderer();
        private readonly SettingsManager _settingsManager;
        private int _selectedIndex = 0;

        public SettingsMenu(GameSettings gameSettings, SettingsManager settingsManager) {
            _gameSettings = gameSettings;
            _settingsManager = settingsManager;
            _settingsItems = [
                new EnumSettingsItem<InputMode>("Tryb sterowania", gameSettings, 
                    gs => gs.CurrentInputMode, 
                    (gs, value) => {
                        gs.CurrentInputMode = value;
                        gs.NotifySettingsChanged();
                    })
            ];

            _gameSettings.SettingsChanged += () => _settingsManager.SaveSettings(_gameSettings);
        }

        /// <summary>
        /// Wyświetla menu ustawień i pozwala na zmianę ustawień
        /// </summary>
        public void Show() {
            ConsoleKeyInfo key;

            do {
                Display();
                key = Console.ReadKey(true);

                switch (key.Key) {
                    case ConsoleKey.UpArrow:
                        _selectedIndex = (_selectedIndex - 1 + _settingsItems.Length + 1) % (_settingsItems.Length + 1);
                        break;
                    case ConsoleKey.DownArrow:
                        _selectedIndex = (_selectedIndex + 1) % (_settingsItems.Length + 1);
                        break;
                    case ConsoleKey.LeftArrow:
                        if (_selectedIndex < _settingsItems.Length) {
                            _settingsItems[_selectedIndex].ChangeValue(false);
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (_selectedIndex < _settingsItems.Length) {
                            _settingsItems[_selectedIndex].ChangeValue(true);
                        }
                        break;
                    case ConsoleKey.Enter:
                        if (_selectedIndex == _settingsItems.Length)  return;
                        break;
                    case ConsoleKey.Escape:
                        return;
                }
            } while (true);
        }

        private void Display() {
            Console.Clear();
            
            // Display title
            Console.ForegroundColor = ConsoleColor.Green;
            _menuRenderer.DisplayText(Menu<int>.GAME_TITLE_HEADING);

            Console.ResetColor();
            Console.WriteLine();
            _menuRenderer.DisplayTextLine("Ustawienia:");
            Console.WriteLine();

            // Display settings items
            for (int i = 0; i < _settingsItems.Length; i++) {
                SettingsItem item = _settingsItems[i];
				int optionLength = item.Name.Length + item.CurrentValueDisplay.Length + 9;
				string padding = new(' ', (MenuRenderer.WIDTH - optionLength) / 2);

                Console.Write($"{padding}{item.Name}:   ");
                Console.Write($"⮜ ");

				if (i == _selectedIndex) {
					Console.BackgroundColor = ConsoleColor.Gray;
					Console.ForegroundColor = ConsoleColor.Black;
				}

                Console.Write($" {item.CurrentValueDisplay} ");
                Console.ResetColor();
                Console.Write($" ⮞{padding}\n");
            }

            Console.WriteLine();
            _menuRenderer.DisplayMenuOption("Wyjdź", _selectedIndex == _settingsItems.Length);
            Console.ResetColor();

            Console.WriteLine();
			Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Użyj strzałek ↑↓ do nawigacji, ←→ do zmiany wartości, Enter lub Esc aby wyjść");
        }
    }
	
	/// <summary>
	/// Klasa przechowująca ustawienia gry
	/// </summary>
	public class GameSettings {
		public InputMode CurrentInputMode { get; set; } = InputMode.Arrow;

		/// <summary>
		/// Event wywoływany gdy jakiekolwiek ustawienie zostanie zmienione
		/// </summary>
		public event Action? SettingsChanged;

		/// <summary>
		/// Metoda do wywołania gdy ustawienie zostanie zmienione
		/// </summary>
		public void NotifySettingsChanged() {
			SettingsChanged?.Invoke();
		}
	}
}
