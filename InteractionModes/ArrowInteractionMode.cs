using SolitaireConsole.UI;
using SolitaireConsole.Input;

namespace SolitaireConsole.InteractionModes {
	public class ArrowInteractionMode : InteractionMode {
		private readonly ArrowInteractionContext context;

		public ArrowInteractionMode(Game game) : this(game, new ArrowInteractionContext()) { }

		private ArrowInteractionMode(Game game, ArrowInteractionContext context)
			: base(new ArrowInputStrategy(game, context), new ConsoleDisplayStrategy(game, context)) {
			this.context = context;
		}
	}

	public struct ArrowInteractionContext { }
}
