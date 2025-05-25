using SolitaireConsole.Input;
using SolitaireConsole.UI;

namespace SolitaireConsole.InteractionModes {
	/// <summary>
	/// Abstrakcyjna klasa bazowa dla trybów interakcji, łącząca strategię wejścia i wyświetlania.
	/// </summary>
	public abstract class InteractionMode(InputStrategy input, DisplayStrategy render) {
		protected InputStrategy InputStrategy { get; } = input;
		protected DisplayStrategy RenderStrategy { get; } = render;

		/// <summary>
		/// Obsługuje wejście użytkownika, przekazując obsługę do strategii wejścia.
		/// </summary>
		/// <param name="indicateGameEnd">Akcja wywoływana przy zakończeniu gry.</param>
		public void HandleInput(Action<GameResult> indicateGameEnd) => InputStrategy.HandleInput(indicateGameEnd);

		/// <summary>
		/// Wyświetla aktualny stan gry za pomocą strategii wyświetlania.
		/// </summary>
		public void Display() => RenderStrategy.Display();

		/// <summary>
		/// Wyświetla podpowiedzi dotyczące możliwych akcji.
		/// </summary>
		public void DisplayHints() {
			RenderStrategy.DisplayHints();
		}
	}
}
