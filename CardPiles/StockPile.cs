using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {
	// Stos rezerwowy (Stock) - skąd dobieramy karty
	public class StockPile : CardPile {
		private readonly Deck deck; // Talia używana do gry
		public override PileType Type { get => PileType.Stock; }

		// Konstruktor inicjalizujący stos rezerwowy z nowej talii
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

		// Metoda do rozdania kart na początku gry do kolumn Tableau
		public List<Card> DealInitialTableauCards(int count) {
			return RemoveTopCards(count);
		}

		// Metoda do dobrania kart ze stosu rezerwowego
		// Zwraca listę dobranych kart (1 lub 3 w zależności od trudności)
		public List<Card> Draw(int drawCount) {
			int actualDrawCount = Math.Min(drawCount, cards.Count); // Nie można dobrać więcej niż jest
			List<Card> drawnCards = RemoveTopCards(actualDrawCount);
			
			// Odkrywamy dobrane karty
			foreach (var card in drawnCards) card.IsFaceUp = true;
			return drawnCards;
		}

		// Metoda do resetowania stosu rezerwowego (przeniesienie kart z Waste z powrotem)
		public void Reset(IEnumerable<Card> wasteCards) {
			AddCards(wasteCards.Reverse()); // Odwracamy kolejność, aby zachować porządek dobierania				
			Shuffle(); // Tasujemy ponownie (zgodnie ze specyfikacją)
		}

		// Prywatna metoda do tasowania kart w stosie rezerwowym używając algorytmu Fisher-Yates.
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
