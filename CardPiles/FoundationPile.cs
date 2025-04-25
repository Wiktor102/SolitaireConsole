using SolitaireConsole.UI;
using SolitaireConsole.Utils;

namespace SolitaireConsole.CardPiles {
	// Stos końcowy (Foundation) - gdzie układamy karty od Asa do Króla
	public class FoundationPile : CardPile {
		public Suit? PileSuit { get; private set; } // Kolor (Suit) tego stosu

		// Sprawdza, czy można dodać kartę na ten stos
		public override bool CanAddCard(Card card) {
			// Jeśli stos jest pusty, można dodać tylko Asa
			if (IsEmpty) {
				return card.Rank == Rank.Ace;
			} else {
				Card? topCard = PeekTopCard(); // Jeśli stos nie jest pusty, sprawdzamy wierzchnią kartę
				if (topCard == null) return false; // Nie powinno się zdarzyć, ale dla bezpieczeństwa

				// Karta musi być tego samego koloru (Suit)
				// i o jeden stopień wyższa (Rank) niż wierzchnia karta
				return card.Suit == topCard.Suit && card.Rank == topCard.Rank + 1;
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

		// Metoda do resetowania koloru stosu (np. przy cofaniu ruchu Asa)
		public void ResetSuitIfEmpty() {
			if (IsEmpty) PileSuit = null; // Teoretycznie moglibyśmy wyrzucić wyjątek jeśli warunek nie jest spełniony
		}

		//public void Display() {
		//	Card? topCard = PeekTopCard();
		//	if (topCard != null) {
		//		topCard.Display();
		//		return;
		//	}

		//	if (!PileSuit.HasValue) {
		//		Console.Write("[   ]"); // Pusty stos
		//		return;
		//	}

		//	// Wyświetl symbol koloru
		//	char suitChar = PileSuit == null ? '?' : (char)PileSuit;
		//	Console.ForegroundColor = PileSuit?.GetColor() ?? ConsoleColor.White;
		//	Console.Write($"[ {PileSuit} ]");
		//	Console.ResetColor();
		//}
		public override PileDisplayInfo GetDisplayInfo() {
			Card? topCard = PeekTopCard();
			List<CardSpot> cardsToDisplay = [];

			if (topCard != null) {
				cardsToDisplay.Add(new CardSpot(topCard));
			} else if (PileSuit != null) {
				cardsToDisplay.Add(new CardSpot((Suit) PileSuit));
			}

			return new PileDisplayInfo {
				CardsToDisplay = cardsToDisplay,
				DisplayDirection = DisplayDirection.Horizontal,
				ShowAmount = null
			};
		}
	}
}