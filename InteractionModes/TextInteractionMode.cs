using SolitaireConsole.Input;
using SolitaireConsole.UI;

namespace SolitaireConsole.InteractionModes {
	public class TextInteractionMode(Game game) : InteractionMode(new TextInputStrategy(game), new ConsoleDisplayStrategy(game)) {
	}
}
