using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {
    // Enum określający typ stosu (do identyfikacji w ruchach)
    public enum PileType { Stock, Waste, Foundation, Tableau }

    public interface IDrawableCardPile {
		void Display(); // Interfejs do wyświetlania stosu
	}

	// Klasa bazowa dla różnych stosów kart (abstrakcyjna)
	public abstract class CardPile {
        protected List<Card> cards = []; // Lista kart na stosie
        public List<Card> Cards => [..cards]; // Publiczna właściwość do dostępu do kart

		// Zwraca liczbę kart na stosie
		public int Count => cards.Count;

        // Sprawdza, czy stos jest pusty
        public bool IsEmpty => cards.Count == 0;

        // Dodaje kartę na wierzch stosu
        public virtual void AddCard(Card card) {
            cards.Add(card);
        }

        // Dodaje listę kart na wierzch stosu
        public virtual void AddCards(IEnumerable<Card> cardsToAdd) {
            cards.AddRange(cardsToAdd);
        }

        // Pobiera kartę z wierzchu stosu (bez usuwania)
        // Zwraca null, jeśli stos jest pusty
        public Card? PeekTopCard() {
            return cards.LastOrDefault();
        }

        // Usuwa i zwraca kartę z wierzchu stosu
        // Zwraca null, jeśli stos jest pusty
        public virtual Card? RemoveTopCard() {
            if (IsEmpty) return null;
            Card card = cards[^1];
            cards.RemoveAt(cards.Count - 1);
            return card;
        }

        // Usuwa i zwraca określoną liczbę kart z wierzchu stosu
        public virtual List<Card> RemoveTopCards(int count) {
            if (count <= 0 || count > cards.Count) return [];
            List<Card> removed = cards.GetRange(cards.Count - count, count);
            cards.RemoveRange(cards.Count - count, count);
            return removed;
        }

        // Czyści stos (usuwa wszystkie karty)
        public void Clear() {
            cards.Clear();
        }

        public override string ToString() {
			return $"{(IsEmpty ? "[   ]" : "[ * ]")} ({Count}) "; // Zwraca [ * ] jeśli są karty, [   ] jeśli pusty
		}

		// Abstrakcyjna metoda sprawdzająca, czy można dodać kartę na ten stos
		// Musi być zaimplementowana przez klasy dziedziczące
		public abstract bool CanAddCard(Card card);

        public abstract PileDisplayInfo GetDisplayInfo();
	}
}