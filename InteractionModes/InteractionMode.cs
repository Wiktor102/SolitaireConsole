using SolitaireConsole.Input;
using SolitaireConsole.UI;

namespace SolitaireConsole.InteractionModes {
	public abstract class InteractionMode(InputStrategy input, DisplayStrategy render) {
		protected InputStrategy InputStrategy { get; } = input;
		protected DisplayStrategy RenderStrategy { get; } = render;

		public void HandleInput(Action<GameResult> indicateGameEnd) => InputStrategy.HandleInput(indicateGameEnd);
		public void Display() => RenderStrategy.Display();
		public void DisplayHints() {
			if (InputStrategy is ArrowInputStrategy) {
				RenderStrategy.DisplayArrowInteractionModeHints();
			} else if (InputStrategy is TextInputStrategy) {
				RenderStrategy.DisplayTextInteractionModeHints();
			} else {
				throw new NotImplementedException("Nieznany typ strategii wejściowej.");
			}
		}
	}
}
