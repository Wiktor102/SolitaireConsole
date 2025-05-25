namespace SolitaireConsole.UI {
	/// <summary>
	/// Klasa reprezentująca menu wyboru z opcjami typu T.
	/// </summary>
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

		/// <summary>
		/// Tworzy nowe menu z nagłówkiem, podtytułem i opcjami.
		/// </summary>
		/// <param name="heading">Nagłówek menu.</param>
		/// <param name="subtitle">Podtytuł menu.</param>
		/// <param name="options">Tablica opcji menu.</param>
		public Menu(string[] heading, string[] subtitle, MenuOption<T>[] options) {
			if (options.Length < 1) throw new ArgumentException("Menu must have at least one option.");
			this.Heading = heading;
			this.Subtitle = subtitle;
			this.options = options;
		}

		/// <summary>
		/// Pozwala użytkownikowi wybrać opcję z menu.
		/// </summary>
		/// <returns>Wybrana wartość typu T.</returns>
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

		/// <summary>
		/// Aktualizuje indeks wybranej opcji w menu.
		/// </summary>
		/// <param name="direction">Kierunek zmiany (1 w dół, -1 w górę).</param>
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

	/// <summary>
	/// Abstrakcyjna klasa do renderowania menu.
	/// </summary>
	public abstract class MenuRenderer {
		public const int WIDTH = 83;
		
		/// <summary>
		/// Wyświetla pojedynczą linię tekstu.
		/// </summary>
		/// <param name="text">Tekst do wyświetlenia.</param>
		public abstract void DisplayTextLine(string text);

		/// <summary>
		/// Wyświetla opcję menu, podświetlając ją jeśli jest wybrana.
		/// </summary>
		/// <param name="text">Tekst opcji.</param>
		/// <param name="selected">Czy opcja jest wybrana.</param>
		public abstract void DisplayMenuOption(string text, bool selected);

		/// <summary>
		/// Wyświetla linię oddzielającą.
		/// </summary>
		public abstract void DisplayDividerLine();

		/// <summary>
		/// Wyświetla tablicę linii tekstu.
		/// </summary>
		/// <param name="text">Tablica linii tekstu.</param>
		public virtual void DisplayText(string[] text) {
			foreach (var line in text) {
				DisplayTextLine(line);
			}
		}
	}

	/// <summary>
	/// Implementacja MenuRenderer wyświetlająca menu w konsoli.
	/// </summary>
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
