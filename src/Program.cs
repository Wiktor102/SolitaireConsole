using SolitaireConsole.UI;
using System.Text;

namespace SolitaireConsole {
	// Główna klasa programu
	class Program {
		static void Main(string[] args) {
			Console.Title = "Konsolowy Pasjans"; // Ustawia tytuł okna konsoli
			Console.OutputEncoding = Encoding.UTF8; // Ważne dla polskich znaków i symboli kart

			// Inicjalizuj menedżera ustawień i wczytaj ustawienia
			SettingsManager settingsManager = new SettingsManager();
			GameSettings gameSettings = settingsManager.LoadSettings();
			bool playAgain = true;
			while (playAgain) {
				var difficultySelector = new DifficultySelector(gameSettings, settingsManager);
				MainMenuAction mainMenuAction = difficultySelector.ShowMainMenu();

				if (mainMenuAction == MainMenuAction.Exit) {
					playAgain = false;
					continue;
				}

				// Jeśli wybrano StartGame, zapytaj o poziom trudności
				DifficultyLevel? difficulty = difficultySelector.ChooseDifficulty();

				if (!difficulty.HasValue) { // Jeśli użytkownik wybrał 'Wróć' w menu trudności
					// playAgain = false; // To zakończyłoby grę, zamiast tego wracamy do menu głównego
					continue; // Wróć na początek pętli while (ponownie pokaż menu główne)
				}

				Game game = new(difficulty.Value, gameSettings); // Rozpocznij nową grę, przekazując gameSettings
				GameResult result = game.RunGameLoop(); // Uruchom główną pętlę gry

				// Obsłuż wynik gry
				switch (result) {
					case GameResult.ShowWinScreen:
						var res = new WinScreen(Game.LastMovesCount).Display();
						if (res == WinScreen.MenuOptions.ShowHighScores) {
							playAgain = new HighScoreScreen().Display();
						} else {
							playAgain = res == WinScreen.MenuOptions.MainMenu;
						}
						break;
					case GameResult.Restart:
						playAgain = true;
						break;
					case GameResult.Quit:
						playAgain = false;
						break;
					case GameResult.Continue:
					default:
						break;
				}
			}

			Console.WriteLine("\nDziękujemy za grę! Do zobaczenia!");
		}
	}
}
