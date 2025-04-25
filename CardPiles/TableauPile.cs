using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {
	// Kolumna gry (Tableau) - 7 kolumn na planszy
	public class TableauPile : CardPile {
		// Konstruktor
		public TableauPile() : base() { }

		// Metoda do inicjalnego rozdania kart do kolumny
		public void DealInitialCards(List<Card> initialCards) {
			cards.AddRange(initialCards);
			// Odkrywamy tylko ostatnią kartę
			if (cards.Count > 0) {
				cards.Last().IsFaceUp = true;
			}
		}

		// Sprawdza, czy można dodać pojedynczą kartę na wierzch tej kolumny
		public override bool CanAddCard(Card card) {
			// Jeśli kolumna jest pusta, można dodać tylko Króla (K)
			if (IsEmpty) {
				return card.Rank == Rank.King;
			} else {
				// Jeśli kolumna nie jest pusta, sprawdzamy wierzchnią kartę
				Card? topCard = PeekTopCard();
				if (topCard != null && topCard.IsFaceUp) // Musi być odkryta
				{
					// Karta musi być przeciwnego koloru (Red/Black)
					// i o jeden stopień niższa (Rank) niż wierzchnia karta
					return card.Color != topCard.Color && card.Rank == topCard.Rank - 1;
				}
				return false; // Nie można dodać na zakrytą kartę
			}
		}

		// Sprawdza, czy można dodać sekwencję kart na wierzch tej kolumny
		// (Pierwsza karta sekwencji musi pasować do wierzchniej karty kolumny)
		public bool CanAddSequence(List<Card> sequence) {
			if (sequence == null || sequence.Count == 0 || !sequence.First().IsFaceUp) {
				return false; // Sekwencja musi istnieć i pierwsza karta musi być odkryta
			}
			// Sprawdza, czy pierwsza karta sekwencji pasuje do wierzchu kolumny
			return CanAddCard(sequence.First());
		}

		// Odkrywa wierzchnią kartę, jeśli jest zakryta
		public bool FlipTopCardIfNecessary() {
			if (!IsEmpty && !cards.Last().IsFaceUp) {
				cards.Last().IsFaceUp = true;
				return true; // Karta została odkryta
			}
			return false; // Karta już była odkryta lub stos jest pusty
		}

		// Znajduje indeks pierwszej odkrytej karty od góry
		// Zwraca -1, jeśli nie ma odkrytych kart
		public int FindFirstFaceUpCardIndex() {
			for (int i = 0; i < cards.Count; i++) {
				if (cards[i].IsFaceUp) {
					return i;
				}
			}
			return -1;
		}

		// Pobiera sekwencję odkrytych kart, zaczynając od podanego indeksu
		public List<Card> GetFaceUpSequence(int startIndex) {
			if (startIndex < 0 || startIndex >= cards.Count || !cards[startIndex].IsFaceUp) {
				return new List<Card>(); // Zwraca pustą listę, jeśli indeks jest nieprawidłowy lub karta jest zakryta
			}
			// Zwraca wszystkie karty od startIndex do końca
			return cards.GetRange(startIndex, cards.Count - startIndex);
		}

		// Usuwa sekwencję kart zaczynającą się od podanego indeksu
		public List<Card> RemoveSequence(int startIndex) {
			if (startIndex < 0 || startIndex >= cards.Count) {
				return new List<Card>();
			}
			int count = cards.Count - startIndex;
			List<Card> removed = cards.GetRange(startIndex, count);
			cards.RemoveRange(startIndex, count);
			return removed;
		}

		// Zwraca listę kart w kolumnie (do wyświetlania)
		public List<Card> GetCardsForDisplay() {
			return cards;
		}
		
		public override PileDisplayInfo GetDisplayInfo() {
			return new PileDisplayInfo {
				CardsToDisplay = [.. cards.Select(c => new CardSpot(c))],
				DisplayDirection = DisplayDirection.Vertical,
				ShowAmount = null
			};
		}
	}
}
