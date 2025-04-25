using System.Text;
using SolitaireConsole.CardPiles;
using SolitaireConsole.Utils;

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
	}

	public class ConsoleDisplayStrategy(Game game) : DisplayStrategy(game) {
		public override void Display() {
			Console.Clear();
			Console.OutputEncoding = Encoding.UTF8; // Ustawienie kodowania dla symboli kart

			// --- Rysowanie górnej części: Stock, Waste, Foundations ---
			Console.WriteLine("--- Pasjans ---");
			Console.Write($"Stos [S]: ");
			DisplayPile(game.Stock.GetDisplayInfo());
			Console.Write("   ");

			Console.Write("Odrzucone [W]: ");
			DisplayPile(game.Waste.GetDisplayInfo());
			Console.Write("   ");

			Console.Write("Stosy Końcowe [F1-F4]: ");
			DisplayPiles([.. game.Foundations.Cast<CardPile>()]);

			Console.WriteLine($"\nLiczba ruchów: {game.MovesCount}");
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca

			// --- Rysowanie kolumn Tableau [T1-T7] ---
			Console.WriteLine("Kolumny Gry [T1-T7]:");
			DisplayPiles([.. game.Tableaux.Cast<CardPile>()]);
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca
		}

		private static void DisplayPiles(List<CardPile> piles) {
			List<List<CardSpot>> matrix = [];
			int i = 0;
			foreach (var pile in piles) {
				var info = pile.GetDisplayInfo();
				if (info.DisplayDirection == DisplayDirection.Horizontal) {
					DisplayHorizontalPile(info.CardsToDisplay, info.ShowAmount);
				} else {
					matrix.Add(info.CardsToDisplay);
				}

				i++;
			}

			if (matrix.Count > 0) DisplayVerticalPiles(matrix);
		}

		private static void DisplayPile(PileDisplayInfo info) {
			if (info.DisplayDirection == DisplayDirection.Horizontal) {
				DisplayHorizontalPile(info.CardsToDisplay, info.ShowAmount);
			} else {
				throw new NotImplementedException();
			}
		}

		private static void DisplayHorizontalPile(List<CardSpot> cardSpots, int? ShowAmount) {
			if (cardSpots.Count == 0) {
				Console.Write("[ ]");
				if (ShowAmount != null) Console.Write($" ({ShowAmount})");
				return;
			}

			int i = 0;
			foreach (var cardSpot in cardSpots) {
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

				if (ShowAmount != null) Console.Write($" ({ShowAmount})");
				if (i < cardSpots.Count - 1) Console.Write(" "); // Odstęp między kartami
				i++;
			}
		}

		public static void DisplayVerticalPiles(List<List<CardSpot>> matrix) {
			int maxRows = 0;
			foreach (var list in matrix) maxRows = Math.Max(maxRows, list.Count);

			for (int row = 0; row < maxRows; row++) {
				foreach (var pile in matrix) {
					if (row < pile.Count) {
						var cardSpot = pile[row];
						if (cardSpot.Card != null) DisplayCard(cardSpot.Card);
					} else {
						Console.Write("   ");
					}

					Console.Write(" ");
				}

				Console.WriteLine();
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
	}
}
