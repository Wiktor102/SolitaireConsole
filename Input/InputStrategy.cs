namespace SolitaireConsole.Input {
	public abstract class InputStrategy(Game game) {
		protected Game game = game;
		public abstract void HandleInput(Action<GameResult> indicateGameEnd);
	}
}
