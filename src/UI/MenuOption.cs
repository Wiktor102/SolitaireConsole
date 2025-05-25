namespace SolitaireConsole.UI {
    public class MenuOption<T>(string label, T value) {
		public string Label { get; } = label;
		public T Value { get; } = value;
	}
}
