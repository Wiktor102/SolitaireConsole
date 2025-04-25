namespace SolitaireConsole.Utils {
	// Enum reprezentujący kolory kart (kier, karo, pik, trefl)
	public enum Suit {
		Hearts = '♥',   // Kier ♥ (Czerwony)
		Diamonds = '♦', // Karo ♦ (Czerwony)
		Clubs = '♣',    // Trefl ♣ (Czarny)
		Spades = '♠'    // Pik ♠ (Czarny)
	}

	public static class SuitExtensions {
		public static ConsoleColor GetColor(this Suit suit) {
			return suit switch {
				Suit.Hearts => ConsoleColor.Red,
				Suit.Diamonds => ConsoleColor.Red,
				Suit.Clubs => ConsoleColor.DarkGray,
				Suit.Spades => ConsoleColor.DarkGray,
				_ => throw new NotImplementedException()
			};
		}
	}
}
