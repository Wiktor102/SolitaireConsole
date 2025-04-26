using System.Text;
using SolitaireConsole.CardPiles;
using SolitaireConsole.Utils;
using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.UI {
	public struct PileDisplayInfo {
		public List<CardSpot> CardsToDisplay;
		public DisplayDirection DisplayDirection;
		public int? ShowAmount;
	}

	public struct CardSpot {
		public Card? Card;
		public Suit? Suit;

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
		public abstract void Display();
		public abstract void DisplayTextInteractionModeHints();
		public abstract void DisplayArrowInteractionModeHints();
	}

	public class ConsoleDisplayStrategy(Game game) : DisplayStrategy(game) {
		private readonly ArrowInteractionContext? _context = null;

		public ConsoleDisplayStrategy(Game game, ArrowInteractionContext context) : this(game) {
			_context = context;
		}

		public override void Display() {
			Console.Clear();
			Console.OutputEncoding = Encoding.UTF8; // Ustawienie kodowania dla symboli kart

			// --- Rysowanie górnej części: Stock, Waste, Foundations ---
			Console.WriteLine("--- Pasjans ---");
			Console.Write($"Stos [S]: ");
			DisplayPiles([game.Stock]);
			Console.Write("   ");

			Console.Write("Odrzucone [W]: ");
			DisplayPiles([game.Waste]);
			Console.Write("   ");

			Console.Write("Stosy Końcowe [F1-F4]: ");
			DisplayPiles([.. game.Foundations.Cast<CardPile>()]);

			Console.WriteLine($"\nLiczba ruchów: {game.MovesCount}");
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca

			// --- Rysowanie kolumn Tableau [T1-T7] ---
			Console.WriteLine("Kolumny Gry [T1-T7]:");
			DisplayPiles([.. game.Tableaux.Cast<CardPile>()]);
			Console.WriteLine("\n" + new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca
		}

		private static void DisplayPiles(List<CardPile> piles) {
			List<List<CardSpot>> matrix = [];
			foreach (var pile in piles) {
				var info = pile.GetDisplayInfo();
				if (info.DisplayDirection == DisplayDirection.Horizontal) {
					// For each card, create a single-item column
					foreach (var cardSpot in info.CardsToDisplay) matrix.Add([cardSpot]);
				} else {
					matrix.Add(info.CardsToDisplay);
				}
			}

			if (matrix.Count > 0) DisplayMatrixOfPiles(matrix);
		}

		public static void DisplayMatrixOfPiles(List<List<CardSpot>> matrix) {
			int maxRows = 0;
			foreach (var list in matrix) maxRows = Math.Max(maxRows, list.Count);

			for (int row = 0; row < maxRows; row++) {
				foreach (var pile in matrix) {
					if (row >= pile.Count) {
						Console.Write("    ");
						continue;
					}

					var cardSpot = pile[row];
					if (cardSpot.Card != null) {
						DisplayCard(cardSpot.Card);
					} else if (cardSpot.Suit != null) {
						Console.Write("[");
						Console.ForegroundColor = ((Suit)cardSpot.Suit).GetColor();
						Console.Write(cardSpot.Suit);
						Console.ResetColor();
						Console.Write("]");
					} else {
						Console.Write("[ ]");
					}

					// TODO: somehow pass info to here
					//if (ShowAmount != null) Console.Write($" ({ShowAmount})");
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
