using SolitaireConsole.CardPiles;
using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.Input {
	public class ArrowInputStrategy(Game game, ArrowInteractionContext context) : InputStrategy(game) {
		private ArrowInteractionContext _context = context;

		public override void HandleInput(Action<GameResult> indicateGameEnd) {
			ConsoleKeyInfo keyInfo = Console.ReadKey(true);
			switch (keyInfo.Key) {
				case ConsoleKey.UpArrow:
					MoveUp();
					break;
				case ConsoleKey.DownArrow:
					MoveDown();
					break;
				case ConsoleKey.LeftArrow:
					MoveLeft();
					break;
				case ConsoleKey.RightArrow:
					MoveRight();
					break;
				case ConsoleKey.Enter:
					if (_context.selectedArea == PileType.Tableau) {
						var tableau = game.Tableaux[_context.selectedTableauIndex!.Value];
						if (tableau.Count > 0) {
							//game.MoveCard(tableau, _context.selectedCardIndex);
						} else {
							Console.WriteLine("Nie można ruszać z pustego tableau.");
						}
					} else if (_context.selectedArea == PileType.Stock) {
						game.DrawFromStock();
					} else if (_context.selectedArea == PileType.Waste) {
						//game.MoveCard(game.Waste, _context.selectedCardIndex);
					}
					break;
				default:
					Console.WriteLine("Invalid key pressed.");
					break;
			}
		}

		private void MoveRight() {
			if (_context.selectedArea == PileType.Tableau) {
				UpdateTableauSelection(_context.selectedTableauIndex!.Value + 1);
			} else if (_context.selectedArea == PileType.Stock) {
				int wasteCount = game.Waste.Cards.Count;
				if (wasteCount == 0) return;

				_context.selectedArea = PileType.Waste;
				_context.selectedCardIndex = 0;
			} else if (_context.selectedArea == PileType.Waste) {
				int wasteCount = game.Waste.Cards.Count;
				_context.selectedCardIndex = Math.Min(_context.selectedCardIndex + 1, wasteCount - 1);
			}
		}

		private void MoveLeft() {
			if (_context.selectedArea == PileType.Tableau) {
				UpdateTableauSelection(_context.selectedTableauIndex!.Value - 1);
			} else if (_context.selectedArea == PileType.Waste) {
				if (_context.selectedCardIndex == 0) {
					_context.selectedArea = PileType.Stock;
					return;
				}

				_context.selectedCardIndex = Math.Max(_context.selectedCardIndex - 1, 0);
			}
		}

		private void MoveUp() {
			if (_context.selectedArea == PileType.Tableau) {
				if (_context.selectedCardIndex == 0) {
					_context.selectedArea = _context.selectedTableauIndex!.Value switch {
						0 or 1 or 2 or 3 => PileType.Stock,
						_ => game.Waste.Cards.Count > 0 ? PileType.Waste : PileType.Stock
					};

					_context.selectedCardIndex = 0;
					return;
				}

				UpdateCardSelection(_context.selectedCardIndex - 1);
			}
		}

		private void MoveDown() {
			if (_context.selectedArea == PileType.Tableau) {
				UpdateCardSelection(_context.selectedCardIndex + 1);
			} else {
				_context.selectedCardIndex = 0;
				_context.selectedTableauIndex = _context.selectedArea switch {
					PileType.Stock => 2,
					PileType.Waste => Math.Min(4 + _context.selectedCardIndex, 6),
					PileType.Foundation => 6, // Foundation nigdy nie może być wybrany -> tylko dla kopilatora
					_ => 0 // Tylko dla kopilatora
				};
				_context.selectedArea = PileType.Tableau;
			}
		}

		private bool UpdateTableauSelection(int newIndex) {
			var clampedIndex = Math.Clamp(newIndex, 0, 6);
			bool changed = _context.selectedTableauIndex != clampedIndex;
			_context.selectedTableauIndex = clampedIndex;
			UpdateCardSelection(_context.selectedCardIndex);
			return changed;
		}

		private bool UpdateCardSelection(int newIndex) {
			var maxIndex = game.Tableaux[_context.selectedTableauIndex.Value].Count - 1;
			var clampedIndex = Math.Clamp(newIndex, 0, maxIndex);
			bool changed = _context.selectedCardIndex != clampedIndex;
			_context.selectedCardIndex = clampedIndex;
			return changed;
		}
	}
}
