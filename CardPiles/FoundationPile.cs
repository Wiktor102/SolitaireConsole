using SolitaireConsole.UI;
using SolitaireConsole.Utils;

namespace SolitaireConsole.CardPiles {
	// Stos końcowy (Foundation) - gdzie układamy karty od Asa do Króla
	public class FoundationPile() : CardPile {
		public Suit? PileSuit { get; private set; } // Kolor (Suit) tego stosu
		private readonly bool _persistantSuit = false;
		public override PileType Type { get => PileType.Foundation; }

		public FoundationPile(Suit pileSuit) : this() {
			PileSuit = pileSuit;
			_persistantSuit = true;
		}

		// Sprawdza, czy można dodać kartę na ten stos
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

		// Dodaje kartę na stos (nadpisuje bazową metodę)
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

		public override Card? RemoveTopCard() {
			if (cards.Count == 1 && !_persistantSuit) {
				PileSuit = null;
			}

			return base.RemoveTopCard();
		}

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
	}
}