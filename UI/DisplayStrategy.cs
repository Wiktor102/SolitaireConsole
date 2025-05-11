using System.Text;
using SolitaireConsole.CardPiles;
using SolitaireConsole.Utils;
using SolitaireConsole.InteractionModes;
using SolitaireConsole.Input;

namespace SolitaireConsole.UI {
	public struct PileDisplayInfo {
		public List<CardSpot> CardsToDisplay;
		public PileType PileType;
		public DisplayDirection DisplayDirection;
		public int? ShowAmount;
	}

	public class CardSpot {
		public Card? Card;
		public Suit? Suit;

		public CardSpot() { }

		public CardSpot(Suit suit) {
			Card = null;
			Suit = suit;
		}

		public CardSpot(Card? card) {
			Card = card;
			Suit = card?.Suit;
		}
	}

	public enum DisplayDirection {
		Horizontal,
		Vertical
	}

	public abstract class DisplayStrategy {
		protected Game game;
		protected readonly ArrowInteractionContext? _context = null;
		protected readonly List<InputActionHint> actionHints = [];
		protected bool ShowPileIds => _context == null; // W trybie strzałek nie pokazujemy ID stosów

		public DisplayStrategy(Game game) {
			this.game = game;
			actionHints = [
				new("draw / d", "                       Dobierz kartę ze stosu [S]"),
				new("move / m <źr> [cel] [liczba]", "   Przenieś kartę/sekwencję (np. m W T2, m T1 F1, m T3 T5 [liczba])"),
				new("undo / u", "                       Cofnij ostatni ruch (do 3 ruchów)"),
				new("score / h", "                      Pokaż ranking"),
				new("restart / r", "                    Rozpocznij nową grę"),
				new("quit / q", "                       Zakończ grę")
			];
		}

		public DisplayStrategy(Game game, ArrowInteractionContext context) : this(game) {
			_context = context;
			actionHints = new List<InputActionHint> {
				// Enter Key Actions
				new("Enter",
					(g, ctx) => ctx != null && ctx.SelectedDestTableauIndex.HasValue ? $"Potwierdź przeniesienie na kolumnę [T{ctx.SelectedDestTableauIndex.Value + 1}]" : "Potwierdź przeniesienie",
					(g, ctx) => ctx != null && ctx.SelectingDestiantionOnTableau && (ctx.SelectedArea == PileType.Tableau || ctx.SelectedArea == PileType.Waste)),
				new("Enter", "Przenieś na stos końcowy / Wybierz cel na kolumnie",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && !ctx.SelectingDestiantionOnTableau && ctx.IsSelectedCardInTableauFaceUp(g)),
				new("Enter", "Przenieś na stos końcowy / Wybierz cel na kolumnie",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Waste && !ctx.SelectingDestiantionOnTableau && !g.Waste.IsEmpty),
				new("Enter", "Dobierz karty",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Stock && g.CanDrawFromStock()),

				// Escape Key Actions
				new("Esc", "Anuluj wybór celu",
					(g, ctx) => ctx != null && ctx.SelectingDestiantionOnTableau),

				// Up Arrow Key Actions
				new("↑", "Wybierz kartę wyżej",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && ctx.SelectedCardIndex > 0),
				new("↑", "Przejdź do stosu rezerwowego",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && ctx.SelectedCardIndex <= 3),
				new("↑", "Przejdź do stosu kart odrzuconych",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && ctx.SelectedCardIndex > 3 && !g.Waste.IsEmpty),

				// Down Arrow Key Actions
				new("↓", "Wybierz kartę niżej",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && ctx.SelectedCardIndex < g.Tableaux[ctx.SelectedTableauIndex!.Value].Count - 1),
				new("↓", "Przejdź do Kolumn Gry",
					(g, ctx) => ctx != null && (ctx.SelectedArea == PileType.Stock || ctx.SelectedArea == PileType.Waste)),

				// Left Arrow Key Actions
				new("←", "Wybierz cel na kolumnie po lewej",
					(g, ctx) => ctx != null && ctx.SelectingDestiantionOnTableau && ctx.SelectedDestTableauIndex!.Value > 0),
				new("←", "Przejdź do kolumny po lewej",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && !ctx.SelectingDestiantionOnTableau && ctx.SelectedTableauIndex!.Value > 0 && g.Tableaux.Take(ctx.SelectedTableauIndex.Value).Any(t => !t.IsEmpty)),
				new("←", "Przejdź do Stosu",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Waste && !ctx.SelectingDestiantionOnTableau),

				// Right Arrow Key Actions
				new("→", "Wybierz cel na kolumnie po prawej",
					(g, ctx) => ctx != null && ctx.SelectingDestiantionOnTableau && ctx.SelectedDestTableauIndex.HasValue && ctx.SelectedDestTableauIndex.Value < 6),
				new("→", "Przejdź do kolumny po prawej",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Tableau && !ctx.SelectingDestiantionOnTableau && ctx.SelectedTableauIndex.HasValue && ctx.SelectedTableauIndex.Value < 6 && g.Tableaux.Skip(ctx.SelectedTableauIndex.Value + 1).Any(t => !t.IsEmpty)),
				new("→", "Przejdź do Odrzuconych",
					(g, ctx) => ctx != null && ctx.SelectedArea == PileType.Stock && !ctx.SelectingDestiantionOnTableau && !g.Waste.IsEmpty),


				new("u", "Cofnij ostatni ruch", (g, ctx) => ctx != null && g.CanUndoLastMove()),
				new("l", "Pokaż ranking", (g, ctx) => true),
				new("q", "Zakończ grę / rozpocznij od nowa", (g, ctx) => true)
			};
		}

		public abstract void Display();
		public abstract void DisplayHints();

		protected ConsoleColor GetMatrixCellBg(PileDisplayInfo info, int row, int column) {
			const ConsoleColor normalColor = ConsoleColor.Black;
			const ConsoleColor selectedColor = ConsoleColor.Gray;
			const ConsoleColor sourceColor = ConsoleColor.Yellow;
			const ConsoleColor destinationColor = ConsoleColor.Green;

			if (_context == null) return normalColor;
			PileType type = info.PileType;

			if (type == PileType.Tableau) {
				if (_context.SelectingDestiantionOnTableau) {
					// Źródło jest już wybrane -> podświetlmy je na inny kolor
					//                         -> wybieramy talię docelową i też podświetlmy ją na inny kolor (to może być konieczne nawet jeśli źródłem jest inny rodzaj stosu)
					// Do tego źródło może mieć większy zakres niż 1 karta
					if (type == _context.SelectedArea && _context.SelectedTableauIndex == column && _context.SelectedCardIndex <= row) return sourceColor;
					if (_context.SelectedDestTableauIndex == column) return destinationColor;
					return normalColor;
				}

				if (type != _context.SelectedArea) return normalColor;

				// Tableau jest zawsze pionowy (w przyszłości można zaimplementować żeby mógłbyć poziomo)
				return _context.SelectedTableauIndex == column && _context.SelectedCardIndex == row ? selectedColor : normalColor;
			}

			if (type != _context.SelectedArea) return normalColor;
			return _context.SelectedCardIndex == (info.DisplayDirection == DisplayDirection.Vertical ? row : column) ? selectedColor : normalColor;
		}
	}

	public class ConsoleDisplayStrategy : DisplayStrategy {
		public ConsoleDisplayStrategy(Game game) : base(game) { }
		public ConsoleDisplayStrategy(Game game, ArrowInteractionContext context) : base(game, context) { }

		public override void Display() {
			Console.Clear();
			Console.OutputEncoding = Encoding.UTF8; // Ustawienie kodowania dla symboli kart

			// --- Rysowanie górnej części: Stock, Waste, Foundations ---
			Console.WriteLine("--- Pasjans ---");
			Console.Write($"Stos{(ShowPileIds ? " [S]" : "")}: ");
			DisplayPiles([game.Stock]);
			Console.Write("   ");

			Console.Write($"Odrzucone{(ShowPileIds ? " [W]" : "")}: ");
			DisplayPiles([game.Waste]);
			Console.Write("   ");

			Console.Write($"Stosy Końcowe{(ShowPileIds ? " [F1-F4]" : "")}: ");
			DisplayPiles([.. game.Foundations.Cast<CardPile>()]);

			Console.WriteLine($"\nLiczba ruchów: {game.MovesCount}");
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca

			// --- Rysowanie kolumn Tableau [T1-T7] ---
			Console.WriteLine($"Kolumny Gry{(ShowPileIds ? " [T1-T7]" : "")}:");
			DisplayPiles([.. game.Tableaux.Cast<CardPile>()]);
			if (_context != null) DisplayTableauSelectionIndicator();
			Console.WriteLine("\n" + new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca

			if (!string.IsNullOrEmpty(game.LastMoveError)) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Błąd: {game.LastMoveError}");
				Console.ResetColor();
				game.ClearLastMoveError(); // Usuń błąd po jego wyswietleniu
			}
		}

		private void DisplayPiles(List<CardPile> piles) {
			// collect full info, not just cards
			var infos = piles.Select(p => p.GetDisplayInfo()).ToList();
			if (infos.Count > 0) DisplayMatrixOfPiles(infos);
		}

		private class CardSportWithPileDisplayInfo : CardSpot {
			public PileDisplayInfo PileInfo { get; }

			public CardSportWithPileDisplayInfo(PileDisplayInfo info, CardSpot cardSpot) : base() {
				PileInfo = info;
				Suit = cardSpot.Suit;
				Card = cardSpot.Card;
			}
		}

		public void DisplayMatrixOfPiles(List<PileDisplayInfo> pileInfos) {
			var matrix = new List<List<CardSportWithPileDisplayInfo>>();
			foreach (var info in pileInfos) {
				if (info.DisplayDirection == DisplayDirection.Horizontal) {
					foreach (var cs in info.CardsToDisplay) matrix.Add([new CardSportWithPileDisplayInfo(info, cs)]);
				} else {
					matrix.Add(info.CardsToDisplay.ConvertAll(cs => new CardSportWithPileDisplayInfo(info, cs)));
				}
			}

			if (matrix.Count == 0) return;
			int maxRows = matrix.Max(col => col.Count);
			for (int row = 0; row < maxRows + Convert.ToInt32(_context != null); row++) { // Jeśli wprowadzanie jest za pomocą strzałek to dodajemy dodatkowy wiersz na strzałkę
				for (int col = 0; col < matrix.Count; col++) {
					var pile = matrix[col];

					if (row >= pile.Count) {
						Console.Write("    ");
						continue;
					}

					var cardSpot = pile[row];
					var pileInfo = cardSpot.PileInfo;

					ConsoleColor cellColor = GetMatrixCellBg(pileInfo, row, col);
					Console.BackgroundColor = cellColor;

					if (cardSpot.Card != null) {
						bool isPlayable = true;
						if (pileInfo.PileType == PileType.Waste && game.Difficulty == DifficultyLevel.Hard) {
							isPlayable = cardSpot.Card == game.Waste.GetPlayableCard();
						}

						DisplayCard(cardSpot.Card, isPlayable);
					} else if (cardSpot.Suit != null) {
						Console.Write("[");
						var fg = Console.ForegroundColor;
						Console.ForegroundColor = cardSpot.Suit.Value.GetColor();
						Console.Write((char)cardSpot.Suit);
						Console.ForegroundColor = fg;
						Console.Write("]");
					} else {
						Console.Write("[ ]");
					}

					Console.ResetColor();

					if ((pileInfo.DisplayDirection == DisplayDirection.Vertical || col == pileInfo.CardsToDisplay.Count - 1) && pileInfo.ShowAmount != null) {
						Console.Write($" ({pileInfo.ShowAmount})");
					}

					Console.Write(" ");
				}

				if (row < maxRows - 1) Console.WriteLine();
			}
		}

		private void DisplayTableauSelectionIndicator() {
			if (!_context!.SelectingDestiantionOnTableau) {
				Console.Write("\n" + new string(' ', 4 * 7));
				return;
			}

			// TODO: Add validation
			Console.Write("\n" + new string(' ', 4 * _context.SelectedDestTableauIndex!.Value));
			Console.Write(" ^  ");
			//Console.Write("\n" + new string(' ', 4 * (6 - _context.SelectedDestTableauIndex!.Value)));
		}

		private static void DisplayCard(Card card, bool isPlayable = true) {
			if (card.IsFaceUp) {
				if (isPlayable) {
					Console.ForegroundColor = card.Suit.GetColor();
				} else {
					Console.ForegroundColor = ConsoleColor.Gray; // Jasny szary dla kart, któych nie można zagrać
				}
			} else {
				Console.ForegroundColor = ConsoleColor.White; // Kolor dla zakrytych kart
			}

			Console.Write(card);
			Console.ResetColor(); // Resetuj kolor po wyświetleniu karty
		}

		public override void DisplayHints() {
			Console.WriteLine("\nAkcje:");
			foreach (var hint in actionHints) {
				if (hint.IsAvailable(game, _context)) {
					Console.WriteLine($" - {hint.KeySymbol} : {hint.GetDescription(game, _context)}");
				}
			}
		}

	}
}
