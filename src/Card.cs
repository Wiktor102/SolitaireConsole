using SolitaireConsole.Utils;

namespace SolitaireConsole {
	/// <summary>
	/// Enum reprezentujący kolory kart (Czerwony/Czarny).
	/// </summary>
	public enum CardColor {
		Red,
		Black
	}

	/// <summary>
	/// Enum reprezentujący figury kart (As, 2-10, Walet, Dama, Król).
	/// </summary>
	public enum Rank {
		Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
	}

	/// <summary>
	/// Klasa reprezentująca pojedynczą kartę.
	/// </summary>
	public class Card(Suit suit, Rank rank) {
		/// <summary>
		/// Kolor karty (kier, karo, pik, trefl).
		/// </summary>
		public Suit Suit { get; } = suit;
		/// <summary>
		/// Figura karty.
		/// </summary>
		public Rank Rank { get; } = rank;
		/// <summary>
		/// Czy karta jest odkryta.
		/// </summary>
		public bool IsFaceUp { get; set; } = false; // Domyślnie karta jest zakryta

		/// <summary>
		/// Zwraca kolor karty (Czerwony/Czarny) na podstawie jej koloru (Suit).
		/// </summary>
		public CardColor Color => (Suit == Suit.Hearts || Suit == Suit.Diamonds) ? CardColor.Red : CardColor.Black;

		/// <summary>
		/// Zwraca reprezentację tekstową karty (np. " A♥", "10♠", " K♦") lub "[*]" jeśli zakryta.
		/// </summary>
		/// <returns>Reprezentacja tekstowa karty.</returns>
		public override string ToString() {
			if (!IsFaceUp) {
				return "[*]"; // Reprezentacja zakrytej karty
			}

			char suitChar = (char)Suit;
			string rankString = Rank switch {
				Rank.Ace => "A",
				Rank.Jack => "J",
				Rank.Queen => "Q",
				Rank.King => "K",
				_ => ((int)Rank).ToString(),
			};

			// Dodajemy spację dla jednocyfrowych rang dla wyrównania
			return $"{(rankString.Length == 1 ? " " : "")}{rankString}{suitChar}";
		}
	}

}