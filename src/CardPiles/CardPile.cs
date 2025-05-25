using SolitaireConsole.UI;

namespace SolitaireConsole.CardPiles {
	/// <summary>
	/// Enum określający typ stosu kart (do identyfikacji w ruchach).
	/// </summary>
	public enum PileType { Stock, Waste, Foundation, Tableau }

	/// <summary>
	/// Klasa bazowa dla różnych stosów kart (abstrakcyjna).
	/// </summary>
	public abstract class CardPile {
		protected List<Card> cards = []; // Lista kart na stosie

		/// <summary>
		/// Publiczna właściwość do dostępu do kart na stosie.
		/// </summary>
		public List<Card> Cards => [.. cards];

		/// <summary>
		/// Typ stosu (Stock, Waste, Foundation, Tableau).
		/// </summary>
		public abstract PileType Type { get; }

		/// <summary>
		/// Zwraca liczbę kart na stosie.
		/// </summary>
		public int Count => cards.Count;

		/// <summary>
		/// Sprawdza, czy stos jest pusty.
		/// </summary>
		public bool IsEmpty => cards.Count == 0;

		/// <summary>
		/// Dodaje kartę na wierzch stosu.
		/// </summary>
		/// <param name="card">Karta do dodania.</param>
		public virtual void AddCard(Card card) {
			cards.Add(card);
		}

		/// <summary>
		/// Dodaje listę kart na wierzch stosu.
		/// </summary>
		/// <param name="cardsToAdd">Karty do dodania.</param>
		public virtual void AddCards(IEnumerable<Card> cardsToAdd) {
			cards.AddRange(cardsToAdd);
		}

		/// <summary>
		/// Pobiera kartę z wierzchu stosu (bez usuwania).
		/// Zwraca null, jeśli stos jest pusty.
		/// </summary>
		/// <returns>Karta z wierzchu stosu lub null.</returns>
		public Card? PeekTopCard() {
			return cards.LastOrDefault();
		}

		/// <summary>
		/// Usuwa i zwraca kartę z wierzchu stosu.
		/// Zwraca null, jeśli stos jest pusty.
		/// </summary>
		/// <returns>Usunięta karta lub null.</returns>
		public virtual Card? RemoveTopCard() {
			if (IsEmpty) return null;
			Card card = cards[^1];
			cards.RemoveAt(cards.Count - 1);
			return card;
		}

		/// <summary>
		/// Usuwa i zwraca określoną liczbę kart z wierzchu stosu.
		/// </summary>
		/// <param name="count">Liczba kart do usunięcia.</param>
		/// <returns>Lista usuniętych kart.</returns>
		public virtual List<Card> RemoveTopCards(int count) {
			if (count <= 0 || count > cards.Count) return [];
			List<Card> removed = cards.GetRange(cards.Count - count, count);
			cards.RemoveRange(cards.Count - count, count);
			return removed;
		}

		/// <summary>
		/// Czyści stos (usuwa wszystkie karty).
		/// </summary>
		public void Clear() {
			cards.Clear();
		}

		/// <summary>
		/// Zwraca reprezentację tekstową stosu.
		/// </summary>
		/// <returns>Tekstowa reprezentacja stosu.</returns>
		public override string ToString() {
			return $"{(IsEmpty ? "[   ]" : "[ * ]")} ({Count}) "; // Zwraca [ * ] jeśli są karty, [   ] jeśli pusty
		}

		/// <summary>
		/// Abstrakcyjna metoda sprawdzająca, czy można dodać kartę na ten stos.
		/// Musi być zaimplementowana przez klasy dziedziczące.
		/// </summary>
		/// <param name="card">Karta do sprawdzenia.</param>
		/// <returns>Czy można dodać kartę.</returns>
		public abstract bool CanAddCard(Card card);

		/// <summary>
		/// Zwraca informacje o wyświetlaniu stosu.
		/// </summary>
		/// <returns>Informacje o wyświetlaniu stosu.</returns>
		public abstract PileDisplayInfo GetDisplayInfo();
	}
}