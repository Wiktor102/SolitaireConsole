namespace SolitaireConsole.Input {
	public abstract class InputStrategy() {
		public abstract void HandleInput(Action<GameResult> indicateGameEnd);

		public abstract ConsoleKeyInfo ReadKey(); // Added abstract ReadKey method
	}

	public interface IGameInputStrategy { 
		Game Game { get; }
	}
}
