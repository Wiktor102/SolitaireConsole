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

	public class ArrowInteractionContext() {
		public PileType SelectedArea = PileType.Tableau;
		public int? SelectedTableauIndex = 0; // Index kolumny głównego obszaru gry (Tableau)
		public int SelectedCardIndex = 0; // Index karty w wybranym stosie (poziomo, ale pionowo dla Tableau - to nie ma większego znaczenia)

		public bool SelectingDestiantionOnTableau => SelectedDestTableauIndex != null;
		public int? SelectedDestTableauIndex;

		// TODO: Implement actual logic based on game state and rules
		public bool CanSelectOrMove() {
			// Placeholder: Always allow trying to select or initiate a move
			return true; 
		}

		// TODO: Implement actual logic based on game state and rules
		public bool CanCompleteMove() {
			// Placeholder: Allow trying to complete a move if a destination is being selected
			return SelectingDestiantionOnTableau;
		}

		public bool IsSelectedCardInTableauFaceUp(Game game) {
			if (SelectedArea != PileType.Tableau || SelectedTableauIndex == null) return false;
			var tableau = game.Tableaux[SelectedTableauIndex.Value];
			if (tableau.IsEmpty || SelectedCardIndex < 0 || SelectedCardIndex >= tableau.Count) return false;
			return tableau.Cards[SelectedCardIndex].IsFaceUp;
		}
	}
}
