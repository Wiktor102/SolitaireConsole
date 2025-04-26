using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.Input {
	public class ArrowInputStrategy(Game game, ArrowInteractionContext context) : InputStrategy(game) {
		private readonly ArrowInteractionContext _context = context;

		public override void HandleInput(Action<GameResult> indicateGameEnd) {
			ConsoleKeyInfo keyInfo = Console.ReadKey(true);
			switch (keyInfo.Key) {
				case ConsoleKey.UpArrow:
					// Handle up arrow key
					break;
				case ConsoleKey.DownArrow:
					// Handle down arrow key
					break;
				case ConsoleKey.LeftArrow:
					// Handle left arrow key
					break;
				case ConsoleKey.RightArrow:
					// Handle right arrow key
					break;
				default:
					Console.WriteLine("Invalid key pressed.");
					break;
			}
		}
	}
}
