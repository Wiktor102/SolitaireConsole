namespace SolitaireConsole.UI {
	internal class WinScreen(int movesCount) {
		private readonly HighScoreManager _highScoreManager = new("highscores.txt");
		private readonly int _movesCount = movesCount;

		public enum MenuOptions {
			ShowHighScores = 1,
			MainMenu = 2,
			Quit = 3
		}

		private readonly MenuRenderer _renderer = new ConsoleMenuRenderer(); // Kompozycja
		private readonly Menu<MenuOptions> _menu = new(Menu<int>.GAME_TITLE_HEADING, [
				$"Gratulacje! Ułożyłeś pasjansa w {movesCount} ruchów!",
				$"Wpisz swój nick aby zapisać wynik (max 3 znaki):"
			], [
				new("Pokaż tabelę wyników", MenuOptions.ShowHighScores),
				new("Powrót do menu głównego", MenuOptions.MainMenu),
				new("Wyjdź", MenuOptions.Quit)
			]
		);

		public MenuOptions Display() {
			Console.Clear();
			Console.ForegroundColor = ConsoleColor.Green;
			_renderer.DisplayText(_menu.Heading);
			Console.ResetColor();
			_renderer.DisplayText(_menu.Subtitle);
			PromptForNickAndSaveScore();
			Console.Clear();

			return _menu.Select();
		}

		private void PromptForNickAndSaveScore() {
			string initials = Console.ReadLine()?.Trim().ToUpper() ?? "XYZ";
			if (initials.Length > 3) initials = initials.Substring(0, 3);
			if (string.IsNullOrWhiteSpace(initials)) initials = "XYZ";
			_highScoreManager.AddScore(initials, _movesCount);
		}
	}
}
