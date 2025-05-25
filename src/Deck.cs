using SolitaireConsole.Utils;

namespace SolitaireConsole {
	/// <summary>
	/// Reprezentuje talię kart.
	/// </summary>
	public class Deck {
		private readonly List<Card> cards; // Lista kart w talii
		private static readonly Random rng = new(); // Do tasowania

		/// <summary>
		/// Zwraca liczbę kart pozostałych w talii.
		/// </summary>
		public int Count => cards.Count;

		/// <summary>
		/// Tworzy nową talię 52 kart i ją tasuje.
		/// </summary>
		public Deck() {
			cards = [];
			foreach (Suit s in Enum.GetValues(typeof(Suit))) {
				foreach (Rank r in Enum.GetValues(typeof(Rank))) {
					cards.Add(new Card(s, r));
				}
			}

			Shuffle();
		}

		/// <summary>
		/// Tasuje karty w talii algorytmem Fishera-Yatesa.
		/// </summary>
		public void Shuffle() {
			int n = cards.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1); // Losowy indeks
				(cards[n], cards[k]) = (cards[k], cards[n]); // Zamień karty miejscami
			}
		}

		/// <summary>
		/// Wydaje jedną kartę z wierzchu talii.
		/// </summary>
		/// <returns>Wydana karta lub <c>null</c>, jeśli talia jest pusta.</returns>
		public Card? Deal() {
			if (cards.Count == 0) return null;
			Card card = cards[^1];
			cards.RemoveAt(cards.Count - 1);
			return card;
		}

		/// <summary>
		/// Dodaje listę kart z powrotem do talii (np. ze stosu odrzuconych).
		/// </summary>
		/// <param name="cardsToAdd">Karty do dodania do talii.</param>
		public void AddCards(IEnumerable<Card> cardsToAdd) {
			foreach (var card in cardsToAdd) {
				card.IsFaceUp = false; // Zakryj karty przed dodaniem ich do talii
				cards.Add(card);
			}
		}
	}

}
