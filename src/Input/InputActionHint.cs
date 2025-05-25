using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.Input {
	/// <summary>
	/// Reprezentuje podpowiedź akcji wejściowej, zawierającą symbol klawisza, opis oraz warunek dostępności.
	/// </summary>
	public class InputActionHint {
		public string KeySymbol { get; }
		private Func<Game, ArrowInteractionContext?, string> DescriptionProvider { get; }
		private Func<Game, ArrowInteractionContext?, bool> IsAvailableCondition { get; }

		/// <summary>
		/// Tworzy nową podpowiedź akcji wejściowej z dynamicznym opisem i warunkiem dostępności.
		/// </summary>
		/// <param name="keySymbol">Symbol klawisza.</param>
		/// <param name="descriptionProvider">Funkcja generująca opis.</param>
		/// <param name="isAvailableCondition">Funkcja określająca dostępność.</param>
		public InputActionHint(string keySymbol, Func<Game, ArrowInteractionContext?, string> descriptionProvider, Func<Game, ArrowInteractionContext?, bool> isAvailableCondition) {
			KeySymbol = keySymbol;
			DescriptionProvider = descriptionProvider;
			IsAvailableCondition = isAvailableCondition;
		}

		/// <summary>
		/// Tworzy nową podpowiedź akcji wejściowej ze statycznym opisem i warunkiem dostępności.
		/// </summary>
		/// <param name="keySymbol">Symbol klawisza.</param>
		/// <param name="staticDescription">Statyczny opis.</param>
		/// <param name="isAvailableCondition">Funkcja określająca dostępność.</param>
		public InputActionHint(string keySymbol, string staticDescription, Func<Game, ArrowInteractionContext?, bool> isAvailableCondition)
			: this(keySymbol, (g, ctx) => staticDescription, isAvailableCondition) { }

		/// <summary>
		/// Tworzy nową podpowiedź akcji wejściowej ze statycznym opisem, zawsze dostępną.
		/// </summary>
		/// <param name="keySymbol">Symbol klawisza.</param>
		/// <param name="staticDescription">Statyczny opis.</param>
		public InputActionHint(string keySymbol, string staticDescription)
			: this(keySymbol, (g, ctx) => staticDescription, (g, ctx) => true) { }

		/// <summary>
		/// Sprawdza, czy akcja jest dostępna w danym stanie gry i kontekście.
		/// </summary>
		/// <param name="currentGame">Aktualna gra.</param>
		/// <param name="currentContext">Aktualny kontekst interakcji.</param>
		/// <returns>Czy akcja jest dostępna.</returns>
		public bool IsAvailable(Game currentGame, ArrowInteractionContext? currentContext) {
			return IsAvailableCondition(currentGame, currentContext);
		}

		/// <summary>
		/// Zwraca opis akcji dla danego stanu gry i kontekstu.
		/// </summary>
		/// <param name="currentGame">Aktualna gra.</param>
		/// <param name="currentContext">Aktualny kontekst interakcji.</param>
		/// <returns>Opis akcji.</returns>
		public string GetDescription(Game currentGame, ArrowInteractionContext? currentContext) {
			return DescriptionProvider(currentGame, currentContext);
		}
	}
}
