namespace SolitaireConsole {
	/// <summary>
	/// Poziom trudnoœci gry
	/// </summary>
	public enum DifficultyLevel {
		/// <summary>
		/// Tryb ³atwy - dobieranie 1 karty
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
		/// Kontynuuj grê
		/// </summary>
		Continue,

		/// <summary>
		/// Uruchom grê ponownie
		/// </summary>
		Restart,

		/// <summary>
		/// Zakoñcz grê
		/// </summary>
		Quit
	}
}
