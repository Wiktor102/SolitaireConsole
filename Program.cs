using System.Text;

namespace SolitaireConsole {
	// Główna klasa programu
	class Program {
		static void Main(string[] args) {
			Console.Title = "Konsolowy Pasjans"; // Ustawia tytuł okna konsoli
			Console.OutputEncoding = Encoding.UTF8; // Ważne dla polskich znaków i symboli kart

			bool playAgain = true;
			while (playAgain) {
				var difficultySelector = new DifficultySelector();
				DifficultyLevel? difficulty = difficultySelector.ChooseDifficulty();
				if (!difficulty.HasValue) {
					playAgain = false;
					continue;
				}

				Game game = new(difficulty.Value); // Rozpocznij nową grę
				GameResult result = game.RunGameLoop(); // Uruchom główną pętlę gry

				// Obsłuż wynik gry
				switch (result) {
					case GameResult.Restart:
						Console.WriteLine("\nRozpoczynanie nowej gry...");
						Thread.Sleep(1000);
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
