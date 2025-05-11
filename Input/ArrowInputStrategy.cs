using SolitaireConsole.CardPiles;
using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.Input {
	public class ArrowInputStrategy(Game game, ArrowInteractionContext context) : InputStrategy(game) {
		private readonly ArrowInteractionContext _context = context;

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
					Enter();
					break;
				case ConsoleKey.Escape:
					Escape();
					break;
				case ConsoleKey.Q: // Added for consistency with text mode
					Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
					if (Console.ReadKey().KeyChar == 't' || Console.ReadKey().KeyChar == 'T') {
						indicateGameEnd(GameResult.Quit);
					}
					break;
				default:
					break;
			}
		}

		private void Enter() {
			game.ClearLastMoveError(); // Wyczyść błędy przed wykonaniem ruchu

			if (_context.SelectedArea == PileType.Tableau) {
				TableauPile tableau = game.Tableaux[_context.SelectedTableauIndex!.Value];
				Card selectedCard = tableau.Cards[_context.SelectedCardIndex];
				if (!selectedCard.IsFaceUp) return; // Nie mozna przenosić zakrytej karty

				List<Card> selectedCardSequence = tableau.Cards[_context.SelectedCardIndex..];

				if (_context.SelectingDestiantionOnTableau) {
					int scrcIndex = _context.SelectedTableauIndex!.Value;
					int destIndex = _context.SelectedDestTableauIndex!.Value;
					_context.SelectedDestTableauIndex = null;

					if (game.TryMove(PileType.Tableau, scrcIndex, PileType.Tableau, destIndex, selectedCardSequence.Count)) {
						RevalidateTableauSelection();
					}

					return;
				}

				
				FoundationPile potentialFoundation = FoundationPile.GetPileForSuit(game.Foundations, selectedCard.Suit, out var i);

				if (selectedCardSequence.Count == 1 && potentialFoundation.CanAddCard(selectedCard)) { // TODO: To jest niezaimplementowane przy trybie tekstowym -> będzie refactor
					game.TryMove(PileType.Tableau, _context.SelectedTableauIndex!.Value, PileType.Foundation, i, 1);
					RevalidateTableauSelection();
					return;
				}

				_context.SelectedDestTableauIndex = 0;
			} else if (_context.SelectedArea == PileType.Stock) {
				game.DrawFromStock(); // TODO: Error message for DrawFromStock is handled internally or via LastMoveError
			} else if (_context.SelectedArea == PileType.Waste) {
				if (_context.SelectingDestiantionOnTableau) {
					int destIndex = _context.SelectedDestTableauIndex!.Value;
					_context.SelectedDestTableauIndex = null;

					if (game.TryMove(PileType.Waste, 0, PileType.Tableau, destIndex, 1)) RevalidateWasteSelection();
					return;
				}

				Card? wasteTopCard = game.Waste.PeekTopCard();
				if (wasteTopCard != null) {
					FoundationPile potentialFoundation = FoundationPile.GetPileForSuit(game.Foundations, wasteTopCard.Suit, out var i);
					if (potentialFoundation.CanAddCard(wasteTopCard)) {
						if (game.TryMove(PileType.Waste, 0, PileType.Foundation, i, 1)) RevalidateWasteSelection();
						return;
					}
				} else {
					game.SetLastMoveError("Stos kart odrzuconych jest pusty.");
				}

				_context.SelectedDestTableauIndex = 0;
			}
		}

		private void Escape() {
			// Sometimes doesn't work?
			if (_context.SelectingDestiantionOnTableau) {
				_context.SelectedDestTableauIndex = null;
				return;
			}
		}

		private void MoveRight() {
			if (_context.SelectingDestiantionOnTableau) {
				_context.SelectedDestTableauIndex = Math.Clamp(_context.SelectedDestTableauIndex!.Value + 1, 0, 6);
				return;
			}

			if (_context.SelectedArea == PileType.Tableau) {
				UpdateTableauSelection(1);
			} else if (_context.SelectedArea == PileType.Stock) {
				int wasteCount = game.Waste.Cards.Count;
				if (wasteCount == 0) return;

				_context.SelectedArea = PileType.Waste;
				_context.SelectedCardIndex = 0;
			} else if (_context.SelectedArea == PileType.Waste) {
				int wasteCount = game.Waste.Cards.Count;
				_context.SelectedCardIndex = Math.Min(_context.SelectedCardIndex + 1, wasteCount - 1);
			}
		}

		private void MoveLeft() {
			if (_context.SelectingDestiantionOnTableau) {
				_context.SelectedDestTableauIndex = Math.Clamp(_context.SelectedDestTableauIndex!.Value - 1, 0, 6);
				return;
			}

			if (_context.SelectedArea == PileType.Tableau) {
				UpdateTableauSelection(-1);
			} else if (_context.SelectedArea == PileType.Waste) {
				if (_context.SelectedCardIndex == 0) {
					_context.SelectedArea = PileType.Stock;
					return;
				}

				_context.SelectedCardIndex = Math.Max(_context.SelectedCardIndex - 1, 0);
			}
		}

		private void MoveUp() {
			if (_context.SelectedArea == PileType.Tableau) {
				if (_context.SelectedCardIndex == 0) {
					_context.SelectedArea = _context.SelectedTableauIndex!.Value switch {
						0 or 1 or 2 or 3 => PileType.Stock,
						_ => game.Waste.Cards.Count > 0 ? PileType.Waste : PileType.Stock
					};

					_context.SelectedCardIndex = 0;
					return;
				}

				UpdateTableauCardSelection(_context.SelectedCardIndex - 1);
			}
		}

		private void MoveDown() {
			if (_context.SelectedArea == PileType.Tableau) {
				UpdateTableauCardSelection(_context.SelectedCardIndex + 1);
			} else {
				_context.SelectedCardIndex = 0;
				_context.SelectedTableauIndex = _context.SelectedArea switch {
					PileType.Stock => 2,
					PileType.Waste => Math.Min(4 + _context.SelectedCardIndex, 6),
					PileType.Foundation => 6, // Foundation nigdy nie może być wybrany -> tylko dla kopilatora
					_ => 0 // Tylko dla kopilatora
				};
				_context.SelectedArea = PileType.Tableau;
			}
		}

		private bool UpdateTableauSelection(int direction) {
			int nextIndex = FindNextNonEmptyTableau(_context.SelectedTableauIndex!.Value, direction);
			if (nextIndex == _context.SelectedTableauIndex!.Value) return false;

			_context.SelectedTableauIndex = nextIndex;
			UpdateTableauCardSelection(0);
			return true;
		}

		private int FindNextNonEmptyTableau(int currentIndex, int direction) {
			int index = currentIndex;
			
			while (true) {
				int nextIndex = index + direction;
				if (nextIndex < 0 || nextIndex > 6) return currentIndex;
				if (!game.Tableaux[nextIndex].IsEmpty) return nextIndex;
				index = nextIndex;
			}
		}

		private bool UpdateTableauCardSelection(int newIndex) {
            if (_context.SelectedArea != PileType.Tableau) return false;
			var maxIndex = game.Tableaux[_context.SelectedTableauIndex!.Value].Count - 1;
			var clampedIndex = Math.Clamp(newIndex, 0, maxIndex);
			bool changed = _context.SelectedCardIndex != clampedIndex;
			_context.SelectedCardIndex = clampedIndex;
			return changed;
		}

		private void RevalidateTableauSelection() {
			if (_context.SelectedArea != PileType.Tableau) return;
			if (!game.Tableaux[_context.SelectedTableauIndex!.Value].IsEmpty) {
				// jeśli wybrany stos nie jest jeszcze pusty, to tylko zmieniamy wybraną kartę na tą wyżej
				UpdateTableauCardSelection(_context.SelectedCardIndex - 1);
				return;
			}

			int currentIndex = _context.SelectedTableauIndex.Value;
			int nearestIndex = currentIndex;
			bool found = false;

			// Szukanie najbliższej niepustej tali kart
			for (int offset = 1; offset < game.Tableaux.Count && !found; offset++) {
				int left = currentIndex - offset;
				int right = currentIndex + offset;

				if (left >= 0 && !game.Tableaux[left].IsEmpty) {
					nearestIndex = left;
					found = true;
				} else if (right < game.Tableaux.Count && !game.Tableaux[right].IsEmpty) {
					nearestIndex = right;
					found = true;
				}
			}

			_context.SelectedTableauIndex = nearestIndex;
			_context.SelectedCardIndex = 0;
		}

		private void RevalidateWasteSelection() {
			if (_context.SelectedArea != PileType.Waste) return;
			if (game.Waste.IsEmpty) {
				_context.SelectedArea = PileType.Stock;
				_context.SelectedCardIndex = 0;
				return;
			}

			// TODO: Do sprawdzenia po naprawieniu tego, że tylko wierzchnią można zagrać
			int wasteCount = game.Waste.Cards.Count;
			_context.SelectedCardIndex = Math.Clamp(_context.SelectedCardIndex, 0, wasteCount - 1);
		}
	}
}
