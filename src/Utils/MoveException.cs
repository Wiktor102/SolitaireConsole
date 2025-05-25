namespace SolitaireConsole.Utils {
	public abstract class MoveException(string message) : Exception(message) {
	}

	public class InvalidPileException(string message) : MoveException(message) {
	}

	public class EmptyPileException(string message) : MoveException(message) {
	}

	public class InvalidMoveRuleException(string message) : MoveException(message) {
	}

	public class InvalidCardSequenceException(string message) : MoveException(message) {
	}
}