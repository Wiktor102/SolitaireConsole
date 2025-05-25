using SolitaireConsole.Utils;

namespace SolitaireConsole {
	/// <summary>
	/// Represents a deck of cards.
	/// </summary>
	public class Deck {
		private readonly List<Card> cards; // List of cards in the deck
		private static readonly Random rng = new(); // For shuffling

		/// <summary>
		/// Gets the number of cards remaining in the deck.
		/// </summary>
		public int Count => cards.Count;

		/// <summary>
		/// Initializes a new instance of the <see cref="Deck"/> class, creating a full shuffled deck of 52 cards.
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
		/// Shuffles the cards in the deck using the Fisher-Yates algorithm.
		/// </summary>
		public void Shuffle() {
			int n = cards.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1); // Random index
				(cards[n], cards[k]) = (cards[k], cards[n]); // Swap cards
			}
		}

		/// <summary>
		/// Deals one card from the top of the deck.
		/// </summary>
		/// <returns>The dealt card, or <c>null</c> if the deck is empty.</returns>
		public Card? Deal() {
			if (cards.Count == 0) return null;
			Card card = cards[^1];
			cards.RemoveAt(cards.Count - 1);
			return card;
		}

		/// <summary>
		/// Adds a list of cards back to the deck (e.g., from the waste pile).
		/// </summary>
		/// <param name="cardsToAdd">The cards to add to the deck.</param>
		public void AddCards(IEnumerable<Card> cardsToAdd) {
			foreach (var card in cardsToAdd) {
				card.IsFaceUp = false; // Hide cards before adding them to the deck
				cards.Add(card);
			}
		}
	}

}
