namespace SolitaireConsole.Utils {
	/// <summary>
	/// Enum reprezentujący kolory kart (kier, karo, pik, trefl).
	/// </summary>
	public enum Suit {
		Hearts = '♥',   // Kier ♥ (Czerwony)
		Diamonds = '♦', // Karo ♦ (Czerwony)
		Clubs = '♣',    // Trefl ♣ (Czarny)
		Spades = '♠'    // Pik ♠ (Czarny)
	}

	/// <summary>
	/// Metody rozszerzające dla typu Suit.
	/// </summary>
	public static class SuitExtensions {
		/// <summary>
		/// Zwraca kolor konsoli odpowiadający danemu kolorowi karty.
		/// </summary>
		/// <param name="suit">Kolor karty.</param>
		/// <returns>Kolor konsoli.</returns>
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
