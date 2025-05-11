namespace SolitaireConsole.UI {
	public abstract class MenuRenderer {
		protected const int WIDTH = 83;
		
		public abstract void DisplayTextLine(string text);
		public abstract void DisplayMenuOption(string text, bool selected);
		public abstract void DisplayDividerLine();
		public virtual void DisplayText(string[] text) {
			foreach (var line in text) {
				DisplayTextLine(line);
			}
		}
	}

	public class ConsoleMenuRenderer : MenuRenderer {
		public override void DisplayTextLine(string text) {
			string padding = new(' ', (WIDTH - text.Length) / 2);
			Console.WriteLine(padding + text + padding);
		}

		public override void DisplayMenuOption(string text, bool selected) {
			string s = "";
			if (selected) {
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.ForegroundColor = ConsoleColor.Black;
				s += "> "; // Wskaźnik wybranej opcji
			} else {
				s += "  ";
			}

			DisplayTextLine(s + text);
			Console.ResetColor(); // Resetuj kolory po każdej opcji
		}

		public override void DisplayDividerLine() {
			Console.WriteLine(new string('-', WIDTH));
		}
	}
}
