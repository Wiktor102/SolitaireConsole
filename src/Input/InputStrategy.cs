namespace SolitaireConsole.Input {
	/// <summary>
	/// Abstrakcyjna klasa strategii wejścia, obsługująca wejście użytkownika.
	/// </summary>
	public abstract class InputStrategy() {
		/// <summary>
		/// Obsługuje wejście użytkownika i sygnalizuje zakończenie gry.
		/// </summary>
		/// <param name="indicateGameEnd">Akcja wskazująca czy należy zakończyć grę.</param>
		public abstract void HandleInput(Action<GameResult> indicateGameEnd);

		/// <summary>
		/// Odczytuje pojedynczy klawisz z wejścia.
		/// </summary>
		/// <returns>Informacje o naciśniętym klawiszu.</returns>
		public abstract ConsoleKeyInfo ReadKey();
	}

	/// <summary>
	/// Interfejs strategii wejścia gry, udostępniający dostęp do gry.
	/// </summary>
	public interface IGameInputStrategy { 
		/// <summary>
		/// Aktualna gra.
		/// </summary>
		Game Game { get; }
	}
}
