namespace SolitaireConsole.UI {
	public class Menu<T> {
		public static readonly string[] GAME_TITLE_HEADING = [
			@" ______   ______     ______       __     ______     __   __     ______   ",
			@"/\  == \ /\  __ \   /\  ___\     /\ \   /\  __ \   /\ ""-.\ \   /\  ___\  ",
			@"\ \  _-/ \ \  __ \  \ \___  \   _\_\ \  \ \  __ \  \ \ \-.  \  \ \___  \ ",
			@" \ \_\    \ \_\ \_\  \/\_____\ /\_____\  \ \_\ \_\  \ \_\\""\_\  \/\_____\",
			@"  \/_/     \/_/\/_/   \/_____/ \/_____/   \/_/\/_/   \/_/ \/_/   \/_____/"
		];

		public string[] Heading;
		public string[] Subtitle;
		protected MenuOption<T>[] options;
		protected int selectedIndex = 0;
		private readonly MenuRenderer _renderer = new ConsoleMenuRenderer(); // Kompozycja

		public Menu(string[] heading, string[] subtitle, MenuOption<T>[] options) {
			if (options.Length < 1) throw new ArgumentException("Menu must have at least one option.");
			this.Heading = heading;
			this.Subtitle = subtitle;
			this.options = options;
		}

		public T Select() {
			ConsoleKeyInfo key;

			do {
				Display();
				key = Console.ReadKey(true);

				switch (key.Key) {
					case ConsoleKey.UpArrow:
						UpdateSelectedIndex(-1);
						break;
					case ConsoleKey.DownArrow:
						UpdateSelectedIndex(1);
						break;
					case ConsoleKey.Enter:
						return options[selectedIndex].Value;
					default:
                        break;
				}
			} while (true);
		}

		private void Display() {
			Console.Clear();

			Console.ForegroundColor = ConsoleColor.Green;
			_renderer.DisplayText(Heading);
			Console.ResetColor();
			Console.WriteLine();
			_renderer.DisplayText(Subtitle);

			for (int i = 0; i < options.Length; i++) {
				_renderer.DisplayMenuOption(options[i].Label, i == selectedIndex);
			}

			_renderer.DisplayDividerLine();
		}

		private void UpdateSelectedIndex(int direction) {
			if (direction < 0) {
				// Przesuń w górę (z zawijaniem)
				selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
			} else {
				// Przesuń w dół (z zawijaniem)
				selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
			}
		}
	}

	public abstract class MenuRenderer {
		public const int WIDTH = 83;
		
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
			string padding = new(' ', (WIDTH - text.Length) / 2);
			
			string s = "";
			if (selected) {
				Console.BackgroundColor = ConsoleColor.Gray;
				Console.ForegroundColor = ConsoleColor.Black;
				s += "> "; // Wskaźnik wybranej opcji
			} else {
				s += "  ";
			}

			Console.WriteLine(padding.Substring(2) + s + text + padding);
			Console.ResetColor(); // Resetuj kolory po każdej opcji
		}

		public override void DisplayDividerLine() {
			Console.WriteLine(new string('-', WIDTH));
		}
	}
}
