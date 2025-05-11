using SolitaireConsole.CardPiles;
using SolitaireConsole.InteractionModes;
using SolitaireConsole.UI;
using SolitaireConsole.Utils;

namespace SolitaireConsole {
	// Główna klasa zarządzająca logiką gry
	public class Game {
		public StockPile Stock { get; private set; }
		public WastePile Waste { get; private set; }
		public List<FoundationPile> Foundations { get; private set; }
		public List<TableauPile> Tableaux { get; private set; }
		public DifficultyLevel Difficulty { get; private set; }
		public int MovesCount { get; private set; } // Licznik ruchów

		private const int MaxUndoSteps = 3; // Maksymalna liczba cofnięć
		private readonly Stack<MoveRecord> _moveHistory; // Stos do przechowywania historii ruchów

		private readonly HighScoreManager _highScoreManager; // Zarządzanie najlepszymi wynikami

		private readonly InteractionMode _interactionMode;
		private readonly MoveService _moveService;

		public string? LastMoveError { get; private set; }

		public Game(DifficultyLevel difficulty) {
			Difficulty = difficulty;
			Stock = new StockPile();
			Waste = new WastePile(difficulty);
			Foundations = new List<FoundationPile>(4);
			Tableaux = new List<TableauPile>(7);
			_moveHistory = new Stack<MoveRecord>();
			MovesCount = 0;
			_highScoreManager = new HighScoreManager("highscores.txt");
			_interactionMode = new ArrowInteractionMode(this);
			_moveService = new MoveService(this);

			// Initialize empty Foundation and Tableau piles
			foreach (Suit suit in Enum.GetValues<Suit>()) Foundations.Add(new FoundationPile(suit));
			for (int i = 0; i < 7; i++) Tableaux.Add(new TableauPile());

			// Deal cards to Tableau piles
			for (int i = 0; i < 7; i++) {
				List<Card> cardsToDeal = Stock.DealInitialTableauCards(i + 1);
				Tableaux[i].DealInitialCards(cardsToDeal);
			}
		}

		// Metoda do obsługi dobierania kart ze stosu rezerwowego (Stock)
		public bool DrawFromStock() {
			// Sprawdź, czy można cofnąć ten ruch (jeśli historia jest pełna)
			bool canUndo = _moveHistory.Count < MaxUndoSteps;

			// Jeśli Stock jest pusty, przenieś karty z Waste z powrotem do Stock
			if (Stock.IsEmpty) {
				if (Waste.IsEmpty) {
					Console.WriteLine("Brak kart do dobrania i stos odrzuconych jest pusty.");
					return false; // Nic się nie dzieje
				}

				// Zapisz stan PRZED resetem (jako specjalny ruch "reset")
				// Tworzymy "pseudo" ruch resetu, aby móc go cofnąć
				// Przechowujemy karty z Waste, które zostaną przeniesione
				var wasteCardsBeforeReset = Waste.Cards;
				// Używamy specjalnych indeksów/typów, aby zidentyfikować reset
				var resetRecord = new MoveRecord(PileType.Waste, -1, PileType.Stock, -1, wasteCardsBeforeReset, false, false);
				if (_moveHistory.Count >= MaxUndoSteps) _moveHistory.Pop(); // Usuń najstarszy ruch, jeśli stos jest pełny
				_moveHistory.Push(resetRecord);


				Stock.Reset(Waste.Cards); // Przenieś karty z Waste
				Waste.Clear(); // Wyczyść Waste
				MovesCount++; // Reset liczy się jako ruch
				return true; // Pomyślnie zresetowano stos
			}

			// Dobierz karty ze Stock do Waste - zmieniono logikę na użycie enuma
			int cardsToDraw = Difficulty == DifficultyLevel.Hard ? 3 : 1;
			List<Card> drawnCards = Stock.Draw(cardsToDraw);

			if (drawnCards.Count > 0) {
				// Zapisz ruch dobrania kart
				// Traktujemy to jako ruch ze Stock do Waste
				// Przechowujemy dobrane karty
				var drawRecord = new MoveRecord(PileType.Stock, 0, PileType.Waste, 0, drawnCards, false, false);
				if (_moveHistory.Count >= MaxUndoSteps) _moveHistory.Pop(); // Usuń najstarszy ruch
				_moveHistory.Push(drawRecord);

				Waste.AddCards(drawnCards); // Dodaj dobrane karty do Waste
				MovesCount++; // Dobranie liczy się jako ruch
				return true; // Pomyślnie dobrano karty
			}
			return false; // Nie udało się dobrać (choć to nie powinno się zdarzyć po sprawdzeniu IsEmpty)
		}

		// Metoda do próby przeniesienia karty lub sekwencji kart
		public bool TryMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, int cardCount = 1) {
			ClearLastMoveError();
			try {
				return _moveService.TryMove(sourceType, sourceIndex, destType, destIndex, cardCount);
			} catch (MoveException ex) {
				SetLastMoveError(ex.Message);
				return false;
			}
		}

		// Metoda do próby automatyczneo przeniesienia karty na stos końcowy (Foundation)
		public bool TryAutoMoveToFoundation(PileType sourceType, int sourceIndex) {
			ClearLastMoveError();
			try {
				return _moveService.TryAutoMoveToFoundation(sourceType, sourceIndex);
			} catch (MoveException ex) {
				SetLastMoveError(ex.Message);
				return false;
			}
		}

		// Metoda do cofania ostatniego ruchu
		public bool UndoLastMove() {
			if (_moveHistory.Count == 0) {
				Console.WriteLine("Brak ruchów do cofnięcia.");
				return false;
			}

			MoveRecord lastMove = _moveHistory.Pop(); // Pobierz ostatni ruch ze stosu

			// --- Logika cofania ruchu ---

			// Sprawdź, czy to był ruch resetu Stock/Waste
			if (lastMove.SourcePileType == PileType.Waste && lastMove.SourcePileIndex == -1 &&
				lastMove.DestPileType == PileType.Stock && lastMove.DestPileIndex == -1) {
				// Cofanie resetu: przenieś karty z powrotem z Stock do Waste
				Stock.Clear(); // Wyczyść Stock
				Waste.Clear(); // Wyczyść Waste (na wszelki wypadek)
							   // Dodaj karty z rekordu z powrotem do Waste (w oryginalnej kolejności)
				Waste.AddCards(lastMove.MovedCards);
				MovesCount--; // Zmniejsz licznik ruchów
				Console.WriteLine("Cofnięto reset stosu.");
				return true;
			}
			// Sprawdź, czy to był ruch dobrania kart (Stock -> Waste)
			else if (lastMove.SourcePileType == PileType.Stock && lastMove.DestPileType == PileType.Waste) {
				// Cofanie dobrania: przenieś karty z powrotem z Waste do Stock
				List<Card> cardsToReturn = new List<Card>();
				// Usuń odpowiednią liczbę kart z Waste (te, które były w MovedCards)
				for (int i = 0; i < lastMove.MovedCards.Count; ++i) {
					Card? card = Waste.RemoveTopCard();
					if (card != null) cardsToReturn.Add(card);
				}
				cardsToReturn.Reverse(); // Muszą wrócić w tej samej kolejności, w jakiej były w Stock

				// Dodaj karty z powrotem do Stock (na wierzch)
				foreach (var card in cardsToReturn) {
					card.IsFaceUp = false; // Zakryj karty wracające do Stock
					Stock.AddCard(card);
				}
				MovesCount--; // Zmniejsz licznik ruchów
				Console.WriteLine("Cofnięto dobranie kart.");
				return true;
			} else // Standardowy ruch między stosami
			  {
				CardPile? sourcePile = GetPile(lastMove.SourcePileType, lastMove.SourcePileIndex);
				CardPile? destPile = GetPile(lastMove.DestPileType, lastMove.DestPileIndex);

				if (sourcePile == null || destPile == null) {
					Console.WriteLine("Błąd wewnętrzny: Nie można znaleźć stosów dla cofnięcia ruchu.");
					// Potencjalnie przywróć ruch do historii? Na razie zakładamy błąd krytyczny.
					return false;
				}

				// 1. Usuń przeniesione karty ze stosu docelowego (Dest)
				// Musimy usunąć dokładnie te karty, które były przeniesione.
				// Najprościej jest usunąć 'n' ostatnich kart, gdzie 'n' to liczba przeniesionych kart.
				List<Card> removedFromDest = destPile.RemoveTopCards(lastMove.MovedCards.Count);

				// Sprawdzenie, czy usunięte karty zgadzają się z zapisanymi (sanity check)
				if (!AreCardListsEqual(removedFromDest, lastMove.MovedCards)) {
					Console.WriteLine("Błąd krytyczny: Niezgodność kart podczas cofania ruchu!");
					// Przywróć stan przed próbą cofnięcia?
					destPile.AddCards(removedFromDest); // Przywróć usunięte karty
					_moveHistory.Push(lastMove); // Przywróć ruch do historii
					return false;
				}


				// 2. Jeśli w stosie źródłowym (Source) odkryto kartę po ruchu, zakryj ją z powrotem
				if (lastMove.WasSourceTopCardFlipped) {
					// Dotyczy tylko Tableau -> Tableau/Foundation
					if (lastMove.SourcePileType == PileType.Tableau) {
						Card? topCard = sourcePile.PeekTopCard();
						if (topCard != null) {
							topCard.IsFaceUp = false;
						} else {
							// To nie powinno się zdarzyć, jeśli WasSourceTopCardFlipped=true
							Console.WriteLine("Ostrzeżenie: Próbowano zakryć kartę na pustym stosie źródłowym podczas cofania.");
						}
					}
				}

				// 3. Dodaj przeniesione karty z powrotem na stos źródłowy (Source)
				// Używamy kart z `lastMove.MovedCards`, bo `removedFromDest` mogło zostać zmodyfikowane
				sourcePile.AddCards(lastMove.MovedCards);


				MovesCount--; // Zmniejsz licznik ruchów
				Console.WriteLine("Cofnięto ostatni ruch.");
				return true;
			}
		}

		public bool CanUndoLastMove() {
			return _moveHistory.Count > 0;
		}

		public bool CanDrawFromStock() {
			// Can always attempt to draw. If stock is empty, it will try to reset from waste.
			// The DrawFromStock method itself handles the logic of empty stock/waste.
			return true; 
		}

		// Internal helper for MoveService to add a move record with undo history handling
		internal void AddMoveRecord(MoveRecord record) {
			if (_moveHistory.Count >= MaxUndoSteps) _moveHistory.Pop();
			_moveHistory.Push(record);
		}

		// Internal helper for MoveService to increment the move counter
		internal void IncrementMoveCount() {
			MovesCount++;
		}

		// Pomocnicza metoda do porównywania list kart (proste porównanie referencji lub wartości)
		private static bool AreCardListsEqual(List<Card> list1, List<Card> list2) {
			if (list1.Count != list2.Count) return false;
			for (int i = 0; i < list1.Count; i++) {
				// Porównujemy Suit i Rank, bo to te same obiekty Card
				if (list1[i].Suit != list2[i].Suit || list1[i].Rank != list2[i].Rank) {
					return false;
				}
			}
			return true;
		}


		// Metoda pomocnicza do pobierania obiektu stosu na podstawie typu i indeksu
		private CardPile? GetPile(PileType type, int index) {
			try {
				switch (type) {
					case PileType.Stock: return Stock;
					case PileType.Waste: return Waste;
					case PileType.Foundation: return Foundations[index]; // index 0-3
					case PileType.Tableau: return Tableaux[index];     // index 0-6
					default: return null;
				}
			} catch (ArgumentOutOfRangeException) {
				// Jeśli indeks jest poza zakresem dla Foundation lub Tableau
				return null;
			}
		}

		// Metoda sprawdzająca warunek zwycięstwa
		public bool CheckWinCondition() {
			// Wygrana następuje, gdy wszystkie 4 stosy końcowe są pełne (mają 13 kart)
			return Foundations.All(f => f.Count == 13);
		}

		// Metoda do obsługi zakończenia gry (wygranej)
		public void HandleWin() {
			_interactionMode.Display(); // Wyświetl stan gry przed zakończeniem
			Console.WriteLine("\n*************************************");
			Console.WriteLine("* Gratulacje! Wygrałeś w Pasjansa! *");
			Console.WriteLine($"* Ukończyłeś grę w {MovesCount} ruchach.    *");
			Console.WriteLine("*************************************\n");

			// Zapisz wynik
			Console.Write("Podaj swoje inicjały (max 3 znaki): ");
			string initials = Console.ReadLine()?.Trim().ToUpper() ?? "XYZ";
			if (initials.Length > 3) initials = initials.Substring(0, 3);
			if (string.IsNullOrWhiteSpace(initials)) initials = "XYZ";

			_highScoreManager.AddScore(initials, MovesCount);
			Console.WriteLine("\nRanking najlepszych wyników:");
			_highScoreManager.DisplayScores();
			Console.WriteLine("\nNaciśnij Enter, aby zakończyć...");
			Console.ReadLine();
		}

		// Metoda do wyświetlania rankingu
		public void DisplayHighScores() {
			Console.WriteLine("\n--- Ranking Najlepszych Wyników ---");
			_highScoreManager.DisplayScores();
			Console.WriteLine("---------------------------------");
		}

		// Metoda głównej pętli gry
		public GameResult RunGameLoop() {
			while (true) {
				_interactionMode.Display(); // Wyświetl stan gry

				// Sprawdź warunek zwycięstwa
				if (CheckWinCondition()) {
					HandleWin(); // Obsłuż wygraną
					return GameResult.Continue; // Domyślnie kontynuujemy do menu głównego po wygranej
				}

				// Wyświetl dostępne akcje
				_interactionMode.DisplayHints();


				GameResult? result = null; // Domyślny wynik
				_interactionMode.HandleInput((r) => result = r); // Obsłuż wejście użytkownika
				if (result != null) return (GameResult)result;
			}
		}

		// Prosta metoda pauzująca grę do czasu naciśnięcia Enter
		public void Pause() {
			Console.WriteLine("\nNaciśnij Enter, aby kontynuować...");
			Console.ReadLine();
		}

		public void ClearLastMoveError() {
			LastMoveError = null;
		}

		public void SetLastMoveError(string? errorMessage) {
			LastMoveError = errorMessage;
		}
	}

	// Klasa reprezentująca pojedynczy ruch (do mechanizmu Undo)
	public class MoveRecord(PileType sourceType, int sourceIndex, PileType destType, int destIndex, List<Card> movedCards, bool flipped, bool foundationSet) {
		public PileType SourcePileType { get; } = sourceType;
		public int SourcePileIndex { get; } = sourceIndex;
		public PileType DestPileType { get; } = destType;
		public int DestPileIndex { get; } = destIndex;
		public List<Card> MovedCards { get; } = movedCards; // Przechowujemy kopię listy
		public bool WasSourceTopCardFlipped { get; } = flipped;
		public bool WasDestFoundationSuitSet { get; } = foundationSet;
	}
}