using SolitaireConsole.UI;
using SolitaireConsole.Input;
using SolitaireConsole.CardPiles;

namespace SolitaireConsole.InteractionModes {
	/// <summary>
	/// Tryb interakcji oparty na sterowaniu strzałkami.
	/// </summary>
	public class ArrowInteractionMode : InteractionMode {
		private readonly ArrowInteractionContext context;

		/// <summary>
		/// Tworzy nowy tryb interakcji strzałkami dla podanej gry.
		/// </summary>
		/// <param name="game">Obiekt gry.</param>
		public ArrowInteractionMode(Game game) : this(game, new ArrowInteractionContext()) { }

		private ArrowInteractionMode(Game game, ArrowInteractionContext context)
			: base(new ArrowInputStrategy(game, context), new ConsoleDisplayStrategy(game, context)) {
			this.context = context;
		}
	}

	/// <summary>
	/// Kontekst interakcji dla trybu sterowania strzałkami.
	/// Przechowuje informacje o aktualnie wybranym obszarze, kolumnie i karcie.
	/// </summary>
	public class ArrowInteractionContext() {
		public PileType SelectedArea = PileType.Tableau;
		public int? SelectedTableauIndex = 0; // Index kolumny głównego obszaru gry (Tableau)
		public int SelectedCardIndex = 0; // Index karty w wybranym stosie (poziomo, ale pionowo dla Tableau - to nie ma większego znaczenia)

		/// <summary>
		/// Określa, czy aktualnie wybierany jest docelowy stos Tableau.
		/// </summary>
		public bool SelectingDestiantionOnTableau => SelectedDestTableauIndex != null;
		public int? SelectedDestTableauIndex;

		/// <summary>
		/// Sprawdza, czy wybrana karta w Tableau jest odkryta.
		/// </summary>
		/// <param name="game">Obiekt gry.</param>
		/// <returns>Czy wybrana karta jest odkryta.</returns>
		public bool IsSelectedCardInTableauFaceUp(Game game) {
			if (SelectedArea != PileType.Tableau || SelectedTableauIndex == null) return false;
			var tableau = game.Tableaux[SelectedTableauIndex.Value];
			if (tableau.IsEmpty || SelectedCardIndex < 0 || SelectedCardIndex >= tableau.Count) return false;
			return tableau.Cards[SelectedCardIndex].IsFaceUp;
		}
	}
}
