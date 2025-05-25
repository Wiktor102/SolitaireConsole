using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {
	/// <summary>
	/// Stos rezerwowy (Stock) - skąd dobieramy karty w grze pasjans.
	/// </summary>
	public class StockPile : CardPile {
		private readonly Deck deck; // Talia używana do gry
		public override PileType Type { get => PileType.Stock; }

		/// <summary>
		/// Konstruktor inicjalizujący stos rezerwowy z nowej, potasowanej talii.
		/// </summary>
		public StockPile() {
			deck = new Deck(); // Tworzy nową, potasowaną talię

			while (deck.Count > 0) { // Przenosi wszystkie karty z talii do stosu rezerwowego
				Card? card = deck.Deal();
				if (card != null) {
					card.IsFaceUp = false; // Karty w stosie rezerwowym są zakryte
					AddCard(card);
				}
			}
		}

		/// <summary>
		/// Rozdaje określoną liczbę kart na początku gry do kolumn Tableau.
		/// </summary>
		/// <param name="count">Liczba kart do rozdania.</param>
		/// <returns>Lista rozdanych kart.</returns>
		public List<Card> DealInitialTableauCards(int count) {
			return RemoveTopCards(count);
		}

		/// <summary>
		/// Dobiera określoną liczbę kart ze stosu rezerwowego.
		/// </summary>
		/// <param name="drawCount">Liczba kart do dobrania.</param>
		/// <returns>Lista dobranych kart (odkrytych).</returns>
		public List<Card> Draw(int drawCount) {
			int actualDrawCount = Math.Min(drawCount, cards.Count); // Nie można dobrać więcej niż jest
			List<Card> drawnCards = RemoveTopCards(actualDrawCount);
			
			// Odkrywamy dobrane karty
			foreach (var card in drawnCards) card.IsFaceUp = true;
			return drawnCards;
		}

		/// <summary>
		/// Resetuje stos rezerwowy, przenosząc karty z Waste z powrotem i tasując je.
		/// </summary>
		/// <param name="wasteCards">Karty do przeniesienia z Waste.</param>
		public void Reset(IEnumerable<Card> wasteCards) {
			AddCards(wasteCards.Reverse()); // Odwracamy kolejność, aby zachować porządek dobierania				
			Shuffle(); // Tasujemy ponownie (zgodnie ze specyfikacją)
		}

		/// <summary>
		/// Tasuje karty w stosie rezerwowym algorytmem Fishera-Yatesa i zakrywa je.
		/// </summary>
		private void Shuffle() {
			int n = cards.Count;
			var rng = new Random();
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				(cards[n], cards[k]) = (cards[k], cards[n]);
			}

			// Upewniamy się, że wszystkie karty są zakryte po przetasowaniu
			foreach (var card in cards) card.IsFaceUp = false;
		}

		// Stos rezerwowy nie przyjmuje kart bezpośrednio (tylko przez Reset)
		public override bool CanAddCard(Card card) {
			return false;
		}

		public override PileDisplayInfo GetDisplayInfo() {
			return new PileDisplayInfo {
				CardsToDisplay = [new CardSpot(PeekTopCard())],
				DisplayDirection = DisplayDirection.Horizontal,
				PileType = Type,
				ShowAmount = cards.Count
			};
		}
	}
}
