using SolitaireConsole.CardPiles;
using SolitaireConsole.InteractionModes;

namespace SolitaireConsole.Input {
	public class ArrowInputStrategy : InputStrategy, IGameInputStrategy {
		private readonly ArrowInteractionContext _context;
		public Game Game { get; private set; }

		public ArrowInputStrategy(Game game, ArrowInteractionContext context) : base() {
			_context = context;
			this.Game = game;
		}
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
					if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0) {
						TryMoveToFoundation();
					} else {
						Enter();
					}
					break;
				case ConsoleKey.Escape:
					Escape();
					break;
				case ConsoleKey.U:
					Game.UndoLastMove();
					break;
				case ConsoleKey.Q:
					Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
					if (Console.ReadKey().KeyChar == 't' || Console.ReadKey().KeyChar == 'T') {
						indicateGameEnd(GameResult.Restart);
					}
					break;
				default:
					break;
			}
		}

		public override ConsoleKeyInfo ReadKey() {
			return Console.ReadKey(true);
		}

		private void Enter() {
			Game.ClearLastMoveError(); // Wyczyść błędy przed wykonaniem ruchu

			if (_context.SelectedArea == PileType.Tableau) {
				TableauPile tableau = Game.Tableaux[_context.SelectedTableauIndex!.Value];
				Card selectedCard = tableau.Cards[_context.SelectedCardIndex];
				if (!selectedCard.IsFaceUp) return; // Check if the selected card is face up before allowing any move

				if (_context.SelectingDestiantionOnTableau) {
					int scrcIndex = _context.SelectedTableauIndex!.Value;
					int destIndex = _context.SelectedDestTableauIndex!.Value;
					_context.SelectedDestTableauIndex = null;

					// Determine card count for T->T move based on selected card index
					List<Card> selectedCardSequence = tableau.Cards[_context.SelectedCardIndex..];
					if (Game.TryMove(PileType.Tableau, scrcIndex, PileType.Tableau, destIndex, selectedCardSequence.Count)) {
						RevalidateTableauSelection();
					}
					return;
				}

				// Attempt auto-move to foundation
				if (Game.TryAutoMoveToFoundation(PileType.Tableau, _context.SelectedTableauIndex!.Value)) {
					RevalidateTableauSelection();
					return;
				}

				// If auto-move to foundation was not possible or failed, and not selecting destination,
				// initiate selection for T->T move.
				_context.SelectedDestTableauIndex = 0;
			} else if (_context.SelectedArea == PileType.Stock) {
				Game.DrawFromStock(); // TODO: Error message for DrawFromStock is handled internally or via LastMoveError
			} else if (_context.SelectedArea == PileType.Waste) {
				if (_context.SelectingDestiantionOnTableau) {
					int destIndex = _context.SelectedDestTableauIndex!.Value;
					_context.SelectedDestTableauIndex = null;

					if (Game.TryMove(PileType.Waste, 0, PileType.Tableau, destIndex, 1)) RevalidateWasteSelection();
					return;
				}

				if (Game.TryAutoMoveToFoundation(PileType.Waste, 0)) {
					RevalidateWasteSelection();
					return;
				}

				Card? wasteTopCard = Game.Waste.PeekTopCard();
				if (wasteTopCard == null) {
					Game.SetLastMoveError("Stos kart odrzuconych jest pusty.");
					return;
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
				int wasteCount = Game.Waste.Cards.Count;
				if (wasteCount == 0) return;

				_context.SelectedArea = PileType.Waste;
				_context.SelectedCardIndex = Game.Difficulty == DifficultyLevel.Hard ? 2 : 0; // W trybie trudnym wierzchnia karta wyświetlana jest jako trzecia
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
				_context.SelectedArea = PileType.Stock;
				_context.SelectedCardIndex = 0; // Na stock zawsze wybieramy wierzchnią kartę
			}
		}

		private void MoveUp() {
			if (_context.SelectingDestiantionOnTableau) return;
			if (_context.SelectedArea == PileType.Tableau) {
				if (_context.SelectedCardIndex == 0) {
					_context.SelectedArea = _context.SelectedTableauIndex!.Value switch {
						0 or 1 or 2 or 3 => PileType.Stock,
						_ => Game.Waste.Cards.Count > 0 ? PileType.Waste : PileType.Stock
					};

					_context.SelectedCardIndex = _context.SelectedArea == PileType.Waste && Game.Difficulty == DifficultyLevel.Hard ? 2 : 0;
					return;
				}

				UpdateTableauCardSelection(_context.SelectedCardIndex - 1);
			}
		}

		private void MoveDown() {
			if (_context.SelectingDestiantionOnTableau) return;
			if (_context.SelectedArea == PileType.Tableau) {
				UpdateTableauCardSelection(_context.SelectedCardIndex + 1);
			} else {
				_context.SelectedArea = PileType.Tableau;
				_context.SelectedCardIndex = 0;

				// Najpierw pobierz docelowy indeks tableau na podstawie bieżącego obszaru
				int targetIndex = _context.SelectedArea switch {
					PileType.Stock => 2,
					PileType.Waste => Math.Min(4 + _context.SelectedCardIndex, 6),
					PileType.Foundation => 6,
					_ => 0
				};

				// Znajdź najbliższą niepustą kolumnę tableau - jeśli docelowa jest pusta, najpierw szukaj w prawo, potem w lewo
				if (targetIndex < Game.Tableaux.Count && !Game.Tableaux[targetIndex].IsEmpty) {
					_context.SelectedTableauIndex = targetIndex;
				} else {
					_context.SelectedTableauIndex = FindNextNonEmptyTableau(targetIndex, 1);
				}
			}
		}

		private bool UpdateTableauSelection(int direction) {
			int nextIndex = FindNextNonEmptyTableau(_context.SelectedTableauIndex!.Value, direction);
			if (nextIndex == _context.SelectedTableauIndex!.Value) return false;

			_context.SelectedTableauIndex = nextIndex;
			UpdateTableauCardSelection(_context.SelectedCardIndex);
			return true;
		}
		private int FindNextNonEmptyTableau(int currentIndex, int direction) {
			int index = currentIndex;
			int searchedCount = 0;

			while (searchedCount < 7) { // Zapobiega nieskończonej pętli, ograniczając wyszukiwanie do liczby kolumn tableau
				int nextIndex = index + direction;
				if (nextIndex < 0 || nextIndex > 6) return currentIndex;
				if (!Game.Tableaux[nextIndex].IsEmpty) return nextIndex;
				index = nextIndex;
				searchedCount++;
			}
			
			// Jeśli po przeszukaniu wszystkich nie znaleziono niepustej kolumny, zwróć bieżący indeks
			return currentIndex;
		}

		private bool UpdateTableauCardSelection(int newIndex) {
			if (_context.SelectedArea != PileType.Tableau) return false;
			var maxIndex = Game.Tableaux[_context.SelectedTableauIndex!.Value].Count - 1;
			var clampedIndex = Math.Clamp(newIndex, 0, maxIndex);
			bool changed = _context.SelectedCardIndex != clampedIndex;
			_context.SelectedCardIndex = clampedIndex;
			return changed;
		}

		private void RevalidateTableauSelection() {
			if (_context.SelectedArea != PileType.Tableau) return;
			if (!Game.Tableaux[_context.SelectedTableauIndex!.Value].IsEmpty) {
				// jeśli wybrany stos nie jest jeszcze pusty, to tylko zmieniamy wybraną kartę na tą wyżej
				UpdateTableauCardSelection(_context.SelectedCardIndex - 1);
				return;
			}

			int currentIndex = _context.SelectedTableauIndex.Value;
			int nearestIndex = currentIndex;
			bool found = false;

			// Szukanie najbliższej niepustej tali kart
			for (int offset = 1; offset < Game.Tableaux.Count && !found; offset++) {
				int left = currentIndex - offset;
				int right = currentIndex + offset;

				if (left >= 0 && !Game.Tableaux[left].IsEmpty) {
					nearestIndex = left;
					found = true;
				} else if (right < Game.Tableaux.Count && !Game.Tableaux[right].IsEmpty) {
					nearestIndex = right;
					found = true;
				}
			}

			_context.SelectedTableauIndex = nearestIndex;
			_context.SelectedCardIndex = 0;
		}

		private void RevalidateWasteSelection() {
			if (_context.SelectedArea != PileType.Waste) return;
			if (Game.Waste.IsEmpty) {
				_context.SelectedArea = PileType.Stock;
				_context.SelectedCardIndex = 0;
				return;
			}

			// Regardless of difficulty, if waste is selected and not empty,
			// the selected card index should always be 0, representing the single playable card.
			_context.SelectedCardIndex = 0;
		}

		private void TryMoveToFoundation() {
			Game.ClearLastMoveError(); // Wyczyść błędy przed wykonaniem ruchu

			if (_context.SelectedArea == PileType.Tableau) {
				TableauPile tableau = Game.Tableaux[_context.SelectedTableauIndex!.Value];
				if (tableau.IsEmpty) return;
				
				Card selectedCard = tableau.Cards[_context.SelectedCardIndex];
				if (!selectedCard.IsFaceUp) return; // Sprawdź, czy wybrana karta jest odkryta przed przeniesieniem

				// Zezwól przeniesienie tylko górnej karty z kolumny Tableau na fundament
				if (_context.SelectedCardIndex == tableau.Cards.Count - 1) {
					if (Game.TryManualMoveToFoundation(PileType.Tableau, _context.SelectedTableauIndex!.Value)) {
						RevalidateTableauSelection();
					}
				} else {
					Game.SetLastMoveError("Można przenieść na fundament tylko wierzchnią kartę z kolumny.");
				}
			} else if (_context.SelectedArea == PileType.Waste) {
				if (Game.TryManualMoveToFoundation(PileType.Waste, 0)) {
					RevalidateWasteSelection();
				}
			} else {
				Game.SetLastMoveError("Przenoszenie na fundament jest możliwe tylko z kolumn głównych gry (Tableau) lub kart odrzuconych (Waste).");
			}
		}
	}
}
