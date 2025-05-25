using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.Input {
	public class InputActionHint {
		public string KeySymbol { get; }
		private Func<Game, ArrowInteractionContext?, string> DescriptionProvider { get; }
		private Func<Game, ArrowInteractionContext?, bool> IsAvailableCondition { get; }

		public InputActionHint(string keySymbol, Func<Game, ArrowInteractionContext?, string> descriptionProvider, Func<Game, ArrowInteractionContext?, bool> isAvailableCondition) {
			KeySymbol = keySymbol;
			DescriptionProvider = descriptionProvider;
			IsAvailableCondition = isAvailableCondition;
		}

		public InputActionHint(string keySymbol, string staticDescription, Func<Game, ArrowInteractionContext?, bool> isAvailableCondition)
			: this(keySymbol, (g, ctx) => staticDescription, isAvailableCondition) { }

		public InputActionHint(string keySymbol, string staticDescription)
			: this(keySymbol, (g, ctx) => staticDescription, (g, ctx) => true) { }

		public bool IsAvailable(Game currentGame, ArrowInteractionContext? currentContext) {
			return IsAvailableCondition(currentGame, currentContext);
		}

		public string GetDescription(Game currentGame, ArrowInteractionContext? currentContext) {
			return DescriptionProvider(currentGame, currentContext);
		}
	}
}
