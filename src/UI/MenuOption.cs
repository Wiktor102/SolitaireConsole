namespace SolitaireConsole.UI {

	/// <summary>
	/// Reprezentuje pojedynczą opcję menu z etykietą i wartością.
	/// </summary>
	public class MenuOption<T>(string label, T value) {
		/// <summary>
		/// Etykieta opcji menu.
		/// </summary>
		public string Label { get; } = label;

		/// <summary>
		/// Wartość powiązana z opcją menu.
		/// </summary>
		public T Value { get; } = value;
	}
}
