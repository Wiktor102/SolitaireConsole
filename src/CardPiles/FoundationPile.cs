using SolitaireConsole.UI;
using SolitaireConsole.Utils;

namespace SolitaireConsole.CardPiles {
	/// <summary>
	/// Stos końcowy (Foundation) - gdzie układamy karty od Asa do Króla.
	/// </summary>
	public class FoundationPile() : CardPile {
		/// <summary>
		/// Kolor (Suit) tego stosu.
		/// </summary>
		public Suit? PileSuit { get; private set; } // Kolor (Suit) tego stosu
		private readonly bool _persistantSuit = false;
		public override PileType Type { get => PileType.Foundation; }

		/// <summary>
		/// Tworzy nowy stos końcowy z przypisanym kolorem.
		/// </summary>
		/// <param name="pileSuit">Kolor stosu</param>
		public FoundationPile(Suit pileSuit) : this() {
			PileSuit = pileSuit;
			_persistantSuit = true;
		}

		/// <summary>
		/// Sprawdza, czy można dodać kartę na ten stos.
		/// </summary>
		/// <param name="card">Karta do sprawdzenia</param>		/// <returns>Prawda, jeśli karta może zostać dodana; w przeciwnym razie fałsz.</returns>
		public override bool CanAddCard(Card card) {
			// Jeśli stos jest pusty, można dodać tylko Asa
			if (IsEmpty) {
				if (_persistantSuit && card.Suit != PileSuit) return false;
				return card.Rank == Rank.Ace;
			} else {
				Card topCard = PeekTopCard()!; // Jeśli stos nie jest pusty, sprawdzamy wierzchnią kartę
				// Karta musi być tego samego koloru (Suit) i o jeden stopień wyższa (Rank) niż wierzchnia karta
				return card.Suit == PileSuit && card.Rank == topCard.Rank + 1;
			}
		}

		/// <summary>
		/// Dodaje kartę na stos (nadpisuje bazową metodę).
		/// </summary>
		/// <param name="card">Karta do dodania</param>
		public override void AddCard(Card card) {
			if (CanAddCard(card)) {
				if (IsEmpty) PileSuit = card.Suit; // Jeśli to pierwsza karta (As), ustawiamy kolor (Suit) stosu
				card.IsFaceUp = true; // Karty na Foundation są zawsze odkryte
				base.AddCard(card);
			} else {
				// Rzucenie wyjątku lub obsługa błędu, jeśli próba dodania nieprawidłowej karty
				// Console.WriteLine("Błąd: Nie można dodać tej karty na stos końcowy.");
			}
		}

		/// <summary>
		/// Usuwa wierzchnią kartę ze stosu.
		/// </summary>
		/// <returns>Usunięta karta lub null, jeśli stos jest pusty</returns>
		public override Card? RemoveTopCard() {
			if (cards.Count == 1 && !_persistantSuit) {
				PileSuit = null;
			}

			return base.RemoveTopCard();
		}

		/// <summary>
		/// Zwraca informacje o wyświetlaniu stosu.
		/// </summary>
		/// <returns>Obiekt PileDisplayInfo z informacjami do wyświetlenia.</returns>
		public override PileDisplayInfo GetDisplayInfo() {
			Card? topCard = PeekTopCard();
			List<CardSpot> cardsToDisplay = [];

			if (topCard != null) {
				cardsToDisplay.Add(new CardSpot(topCard));
			} else if (PileSuit != null) {
				cardsToDisplay.Add(new CardSpot((Suit)PileSuit));
			} else {
				cardsToDisplay.Add(new CardSpot());
			}

			return new PileDisplayInfo {
				CardsToDisplay = cardsToDisplay,
				DisplayDirection = DisplayDirection.Horizontal,
				PileType = Type,
				ShowAmount = null
			};
		}

		/// <summary>
		/// Zwraca stos końcowy odpowiadający danemu kolorowi.
		/// </summary>
		/// <param name="piles">Lista stosów końcowych</param>
		/// <param name="suit">Kolor</param>
		/// <param name="index">Zwracany indeks stosu</param>
		/// <returns>Stos końcowy o danym kolorze lub pierwszy z listy</returns>
		public static FoundationPile GetPileForSuit(List<FoundationPile> piles, Suit suit, out int index) {
			index = 0;

			foreach (FoundationPile pile in piles) {
				if (pile.PileSuit == suit) return pile;
				index++;
			}

			return piles[0];
		}
	}
}