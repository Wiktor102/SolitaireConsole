using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {

	/// <summary>
	/// Stos kart odrzuconych (Waste) - gdzie trafiają karty ze Stock
	/// </summary>
	public class WastePile(DifficultyLevel difficulty) : CardPile {
		private readonly int _numberOfCardsToDisplay = difficulty == DifficultyLevel.Easy ? 1 : 3; // Liczba kart do wyświetlenia w Waste zależna od poziomu trudności
		public override PileType Type { get => PileType.Waste; }

		/// <summary>
		/// Metoda do dodawania kart dobranych ze Stock
		/// </summary>
		/// <param name="cardsToAdd">Karty do dodania</param>
		public override void AddCards(IEnumerable<Card> cardsToAdd) {
			foreach (var card in cardsToAdd) {
				card.IsFaceUp = true; // Karty w Waste są zawsze odkryte
			}

			base.AddCards(cardsToAdd); // Dodaje karty na wierzch
		}

		/// <summary>
		/// Waste Pile nie przyjmuje kart w standardowy sposób (tylko ze Stock)
		/// </summary>
		/// <param name="card">Karta do sprawdzenia</param>
		/// <returns>Zawsze false</returns>
		public override bool CanAddCard(Card card) {
			return false;
		}

		public override PileDisplayInfo GetDisplayInfo() {
			return new PileDisplayInfo {
				CardsToDisplay = [.. cards.TakeLast(_numberOfCardsToDisplay).Select(c => new CardSpot(c))],
				DisplayDirection = DisplayDirection.Horizontal,
				PileType = Type,
				ShowAmount = Cards.Count
			};
		}
	}
}
