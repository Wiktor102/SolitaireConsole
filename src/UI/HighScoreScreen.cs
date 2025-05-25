namespace SolitaireConsole.UI {
	internal class HighScoreScreen {
		private readonly HighScoreManager _highScoreManager;
		private readonly Menu<bool> _menu;

		public HighScoreScreen() {
			_highScoreManager = new HighScoreManager("highscores.txt");
			_menu = new(Menu<int>.GAME_TITLE_HEADING, ["Ranking wyników:", .._highScoreManager.GetDisplayStrings()], [
				new("Powrót do menu głównego", true),
				new("Wyjdź z gry", false),
			]);
		}

		public bool Display() {
			return _menu.Select();
		}
	}
}
