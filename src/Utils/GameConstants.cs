namespace SolitaireConsole {
	/// <summary>
	/// Poziom trudnoœci gry
	/// </summary>
	public enum DifficultyLevel {
		/// <summary>
		/// Tryb łatwy - dobieranie 1 karty
		/// </summary>
		Easy,

		/// <summary>
		/// Tryb trudny - dobieranie 3 kart
		/// </summary>
		Hard
	}

	/// <summary>
	/// Enum do sygnalizowania wyniku zakoñczenia gry
	/// </summary>
	public enum GameResult {
		/// <summary>
		/// Kontynuuj grę
		/// </summary>
		Continue,

		/// <summary>
		/// Przejdź do ekranu końcowego
		/// </summary>
		ShowWinScreen,

		/// <summary>
		/// Uruchom grę ponownie
		/// </summary>
		Restart,

		/// <summary>
		/// Zakończ grę
		/// </summary>
		Quit
	}
}
