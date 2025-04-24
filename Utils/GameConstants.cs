namespace SolitaireConsole {
	/// <summary>
	/// Poziom trudno�ci gry
	/// </summary>
	public enum DifficultyLevel {
		/// <summary>
		/// Tryb �atwy - dobieranie 1 karty
		/// </summary>
		Easy,

		/// <summary>
		/// Tryb trudny - dobieranie 3 kart
		/// </summary>
		Hard
	}

	/// <summary>
	/// Enum do sygnalizowania wyniku zako�czenia gry
	/// </summary>
	public enum GameResult {
		/// <summary>
		/// Kontynuuj gr�
		/// </summary>
		Continue,

		/// <summary>
		/// Uruchom gr� ponownie
		/// </summary>
		Restart,

		/// <summary>
		/// Zako�cz gr�
		/// </summary>
		Quit
	}
}
