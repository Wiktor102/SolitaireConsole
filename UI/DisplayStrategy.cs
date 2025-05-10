using System.Text;
using SolitaireConsole.CardPiles;
using SolitaireConsole.Utils;
using SolitaireConsole.InteractionModes;
using System.IO;

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

		public CardSpot() {}

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

	public abstract class DisplayStrategy(Game game) {
		protected Game game = game;
		protected readonly ArrowInteractionContext? _context = null;
		protected bool ShowPileIds => _context == null; // W trybie strzałek nie pokazujemy ID stosów

		public DisplayStrategy(Game game, ArrowInteractionContext context) : this(game) {
			_context = context;
		}

		public abstract void Display();
		public abstract void DisplayTextInteractionModeHints();
		public abstract void DisplayArrowInteractionModeHints();

		protected bool IsMatrixCellSelected(PileDisplayInfo info, int row, int column) {
			if (_context == null) return false;
			PileType type = info.PileType;

			if (type != _context.selectedArea) return false;
			if (type == PileType.Tableau) return _context.selectedTableauIndex == column && _context.selectedCardIndex == row; // Tableau jest zawsze pionowy (w przyszłości można zaimplementować żeby mógłbyć poziomo)
			return _context.selectedCardIndex == (info.DisplayDirection == DisplayDirection.Vertical ? row : column);
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
			Console.WriteLine("\n" + new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca
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
			for (int row = 0; row < maxRows; row++) {
				for (int col = 0; col < matrix.Count; col++) {
					var pile = matrix[col];
					if (row >= pile.Count) {
						Console.Write("    ");
						continue;
					}

					var cardSpot = pile[row];
					var pileInfo = cardSpot.PileInfo;

					bool isCellSelected = IsMatrixCellSelected(pileInfo, row, col);
					if (isCellSelected) Console.BackgroundColor = ConsoleColor.Gray;

					if (cardSpot.Card != null) DisplayCard(cardSpot.Card);
					else if (cardSpot.Suit != null) {
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

		private static void DisplayCard(Card card) {
			if (card.IsFaceUp) {
				Console.ForegroundColor = card.Suit.GetColor();
			} else {
				Console.ForegroundColor = ConsoleColor.White; // Kolor dla zakrytych kart
			}

			Console.Write(card);
			Console.ResetColor(); // Resetuj kolor po wyświetleniu karty
		}


		public override void DisplayTextInteractionModeHints() {
			Console.WriteLine("\nAkcje:");
			Console.WriteLine(" - draw / d           : Dobierz kartę ze stosu [S]");
			Console.WriteLine(" - move / m [źr] [cel]: Przenieś kartę/sekwencję (np. m W T2, m T1 F1, m T3 T5 [liczba])");
			Console.WriteLine(" - undo / u           : Cofnij ostatni ruch (do 3 ruchów)");
			Console.WriteLine(" - score / h          : Pokaż ranking");
			Console.WriteLine(" - restart / r        : Rozpocznij nową grę");
			Console.WriteLine(" - quit / q           : Zakończ grę");
		}

		public override void DisplayArrowInteractionModeHints() {
			Console.WriteLine("\nAkcje:");
			Console.WriteLine(" - ↑ : Dobierz kartę ze stosu [S]");
			Console.WriteLine(" - ↓ : Przenieś kartę/sekwencję (np. ↓ W T2, ↓ T1 F1, ↓ T3 T5 [liczba])");
			Console.WriteLine(" - ← : Cofnij ostatni ruch (do 3 ruchów)");
			Console.WriteLine(" - → : Pokaż ranking");
			Console.WriteLine(" - Esc: Rozpocznij nową grę");
			Console.WriteLine(" - q  : Zakończ grę");
		}
	}
}
