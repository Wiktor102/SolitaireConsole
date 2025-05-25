using SolitaireConsole.Utils;

namespace SolitaireConsole {
	// Enum reprezentujący kolory kart (Czerwony/Czarny)
	public enum CardColor {
		Red,
		Black
	}

	// Enum reprezentujący figury kart (As, 2-10, Walet, Dama, Król)
	public enum Rank {
		Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
	}

	// Klasa reprezentująca pojedynczą kartę
	public class Card(Suit suit, Rank rank) {
		public Suit Suit { get; } = suit;
		public Rank Rank { get; } = rank;
		public bool IsFaceUp { get; set; } = false; // Domyślnie karta jest zakryta

		// Zwraca kolor karty (Czerwony/Czarny) na podstawie jej koloru (Suit)
		public CardColor Color => (Suit == Suit.Hearts || Suit == Suit.Diamonds) ? CardColor.Red : CardColor.Black;

		// Zwraca reprezentację tekstową karty (np. " A♥", "10♠", " K♦")
		// lub "[ ]" jeśli zakryta
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