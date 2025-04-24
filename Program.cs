using System.Text;

namespace SolitaireConsole {
	// Główna klasa programu
	class Program {
		static void Main(string[] args) {
			Console.Title = "Pasjans Konsolowy"; // Ustawia tytuł okna konsoli
			Console.OutputEncoding = Encoding.UTF8; // Ważne dla polskich znaków i symboli kart

			bool playAgain = true;
			while (playAgain) {
				// Wybór poziomu trudności używając nowej klasy DifficultySelector
				var difficultySelector = new DifficultySelector();
				DifficultyLevel? difficulty = difficultySelector.ChooseDifficulty();
				if (!difficulty.HasValue) {
					playAgain = false;
					continue;
				}

				Game game = new(difficulty.Value); // Rozpocznij nową grę

				// Uruchom główną pętlę gry (teraz w klasie Game)
				Game.GameResult result = game.RunGameLoop();

				// Obsłuż wynik gry
				switch (result) {
					case Game.GameResult.Restart:
						Console.WriteLine("\nRozpoczynanie nowej gry...");
						Thread.Sleep(1000);
						break;
					case Game.GameResult.Quit:
						playAgain = false;
						break;
					case Game.GameResult.Continue:
					default:
						break;
				}
			}

			Console.WriteLine("\nDziękujemy za grę! Do zobaczenia!");
		}
	}
}
