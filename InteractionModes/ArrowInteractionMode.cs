using SolitaireConsole.UI;
using SolitaireConsole.Input;
using SolitaireConsole.CardPiles;

namespace SolitaireConsole.InteractionModes {
	public class ArrowInteractionMode : InteractionMode {
		private readonly ArrowInteractionContext context;

		public ArrowInteractionMode(Game game) : this(game, new ArrowInteractionContext()) { }

		private ArrowInteractionMode(Game game, ArrowInteractionContext context)
			: base(new ArrowInputStrategy(game, context), new ConsoleDisplayStrategy(game, context)) {
			this.context = context;
		}
	}

	public struct ArrowInteractionContext() {
		public PileType selectedArea = PileType.Tableau;
		public int? selectedTableauIndex = 0; // Index kolumny głównego obszaru gry (Tableau)
		public int selectedCardIndex = 0; // Index karty w wybranym stosie (poziomo, ale pionowo dla Tableau - to nie ma większego znaczenia)
	}
}
