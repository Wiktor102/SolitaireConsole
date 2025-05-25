using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {
	/// <summary>
	/// Reprezentuje kolumnę gry (Tableau) w pasjansie – jedną z 7 kolumn na planszy.
	/// </summary>
	public class TableauPile() : CardPile() {
		public override PileType Type { get => PileType.Tableau; }

		/// <summary>
		/// Rozdaje początkowe karty do kolumny i odkrywa ostatnią z nich.
		/// </summary>
		/// <param name="initialCards">Lista kart do rozdania.</param>
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

		/// <summary>
		/// Sprawdza, czy można dodać sekwencję kart na wierzch tej kolumny.
		/// Pierwsza karta sekwencji musi pasować do wierzchniej karty kolumny.
		/// </summary>
		/// <param name="sequence">Sekwencja kart do dodania.</param>
		/// <returns>Czy sekwencja może zostać dodana.</returns>
		public bool CanAddSequence(List<Card> sequence) {
			if (sequence == null || sequence.Count == 0 || !sequence.First().IsFaceUp) {
				return false; // Sekwencja musi istnieć i pierwsza karta musi być odkryta
			}
			// Sprawdza, czy pierwsza karta sekwencji pasuje do wierzchu kolumny
			return CanAddCard(sequence.First());
		}

		/// <summary>
		/// Odkrywa wierzchnią kartę, jeśli jest zakryta.
		/// </summary>
		/// <returns>Zwraca true, jeśli karta została odkryta.</returns>
		public bool FlipTopCardIfNecessary() {
			if (!IsEmpty && !cards.Last().IsFaceUp) {
				cards.Last().IsFaceUp = true;
				return true; // Karta została odkryta
			}
			return false; // Karta już była odkryta lub stos jest pusty
		}

		/// <summary>
		/// Znajduje indeks pierwszej odkrytej karty od góry.
		/// </summary>
		/// <returns>Indeks pierwszej odkrytej karty lub -1 jeśli brak.</returns>
		public int FindFirstFaceUpCardIndex() {
			for (int i = 0; i < cards.Count; i++) {
				if (cards[i].IsFaceUp) {
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Pobiera sekwencję odkrytych kart, zaczynając od podanego indeksu.
		/// </summary>
		/// <param name="startIndex">Indeks początkowy.</param>
		/// <returns>Lista odkrytych kart od podanego indeksu.</returns>
		public List<Card> GetFaceUpSequence(int startIndex) {
			if (startIndex < 0 || startIndex >= cards.Count || !cards[startIndex].IsFaceUp) {
				return new List<Card>(); // Zwraca pustą listę, jeśli indeks jest nieprawidłowy lub karta jest zakryta
			}
			// Zwraca wszystkie karty od startIndex do końca
			return cards.GetRange(startIndex, cards.Count - startIndex);
		}

		/// <summary>
		/// Usuwa sekwencję kart zaczynającą się od podanego indeksu.
		/// </summary>
		/// <param name="startIndex">Indeks początkowy sekwencji do usunięcia.</param>
		/// <returns>Lista usuniętych kart.</returns>
		public List<Card> RemoveSequence(int startIndex) {
			if (startIndex < 0 || startIndex >= cards.Count) {
				return new List<Card>();
			}
			int count = cards.Count - startIndex;
			List<Card> removed = cards.GetRange(startIndex, count);
			cards.RemoveRange(startIndex, count);
			return removed;
		}

		/// <summary>
		/// Zwraca listę kart w kolumnie do wyświetlania.
		/// </summary>
		/// <returns>Lista kart w kolumnie.</returns>
		public List<Card> GetCardsForDisplay() {
			return cards;
		}
		
		public override PileDisplayInfo GetDisplayInfo() {
			return new PileDisplayInfo {
				CardsToDisplay = [.. cards.Select(c => new CardSpot(c))],
				DisplayDirection = DisplayDirection.Vertical,
				PileType = Type,
				ShowAmount = null
			};
		}
	}
}
