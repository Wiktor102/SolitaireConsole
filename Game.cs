using System.Text;

namespace SolitaireConsole {
	// Główna klasa zarządzająca logiką gry
	public class Game {
		public StockPile Stock { get; private set; }
		public WastePile Waste { get; private set; }
		public List<FoundationPile> Foundations { get; private set; }
		public List<TableauPile> Tableaux { get; private set; }
		public DifficultyLevel Difficulty { get; private set; } // Zmieniono z IsHardMode (bool) na Difficulty (enum)	
		public int MovesCount { get; private set; } // Licznik ruchów

		private const int MaxUndoSteps = 3; // Maksymalna liczba cofnięć
		private Stack<MoveRecord> moveHistory; // Stos do przechowywania historii ruchów

		private HighScoreManager highScoreManager; // Zarządzanie najlepszymi wynikami

		 // Zaktualizowany konstruktor używający DifficultyLevel zamiast bool
		public Game(DifficultyLevel difficulty) {
			Difficulty = difficulty;
			Stock = new StockPile();
			Waste = new WastePile();
			Foundations = new List<FoundationPile>(4);
			Tableaux = new List<TableauPile>(7);
			moveHistory = new Stack<MoveRecord>();
			MovesCount = 0;
			highScoreManager = new HighScoreManager("highscores.txt"); // Plik do zapisu wyników

			// Inicjalizacja pustych stosów końcowych i kolumn gry
			for (int i = 0; i < 4; i++) Foundations.Add(new FoundationPile());
			for (int i = 0; i < 7; i++) Tableaux.Add(new TableauPile());

			// Rozdanie kart do kolumn Tableau
			for (int i = 0; i < 7; i++) {
				// Kolumna 'i' otrzymuje 'i+1' kart
				List<Card> cardsToDeal = Stock.DealInitialTableauCards(i + 1);
				Tableaux[i].DealInitialCards(cardsToDeal);
			}
		}

		// Metoda rysująca aktualny stan gry w konsoli
		public void DisplayGame() {
			Console.Clear(); // Czyści konsolę przed rysowaniem
			Console.OutputEncoding = Encoding.UTF8; // Ustawienie kodowania dla symboli kart

			// --- Rysowanie górnej części: Stock, Waste, Foundations ---
			Console.WriteLine("--- Pasjans ---");
			Console.Write($"Stos [S]: {(Stock.IsEmpty ? "[   ]" : "[ * ]")} ({Stock.Count})   "); // Wyświetla [ * ] jeśli są karty, [   ] jeśli pusty
			Console.Write("Odrzucone [W]: ");
			// Wyświetla 1 lub 3 karty z Waste w zależności od trybu
			if (Waste.IsEmpty) {
				Console.Write("[   ]");
			} else {
				var wasteCards = Waste.GetAllCards();
				int startIndex = Difficulty == DifficultyLevel.Hard ? Math.Max(0, wasteCards.Count - 3) : Math.Max(0, wasteCards.Count - 1);
				// W trybie trudnym pokazujemy do 3 kart, w łatwym 1
				// Ale tylko wierzchnia jest grywalna
				for (int i = startIndex; i < wasteCards.Count; i++) {
					DisplayCard(wasteCards[i]);
					Console.Write(" ");
				}
				// Jeśli w trybie trudnym jest mniej niż 3, dopełnij spacjami
				if (Difficulty == DifficultyLevel.Hard && wasteCards.Count < 3) {
					for (int i = 0; i < 3 - wasteCards.Count; ++i) Console.Write("    ");
				}

			}

			Console.Write("    Stosy Końcowe [F1-F4]: ");
			for (int i = 0; i < 4; i++) {
				Card? topCard = Foundations[i].PeekTopCard();
				if (topCard != null) {
					DisplayCard(topCard);
				} else {
					// Wyświetl symbol koloru, jeśli jest ustawiony, inaczej puste miejsce
					Suit? pileSuit = Foundations[i].GetPileSuit();
					if (pileSuit.HasValue) {
						char suitChar = '?';
						ConsoleColor color = ConsoleColor.White;
						switch (pileSuit.Value) {
							case Suit.Hearts: suitChar = '♥'; color = ConsoleColor.Red; break;
							case Suit.Diamonds: suitChar = '♦'; color = ConsoleColor.Red; break;
							case Suit.Clubs: suitChar = '♣'; color = ConsoleColor.DarkGray; break; // Użyj DarkGray dla czarnych
							case Suit.Spades: suitChar = '♠'; color = ConsoleColor.DarkGray; break;
						}
						Console.ForegroundColor = color;
						Console.Write($"[ {suitChar} ]");
						Console.ResetColor();
					} else {
						Console.Write("[   ]"); // Pusty stos
					}
				}
				Console.Write(" ");
			}
			Console.WriteLine($"\nLiczba ruchów: {MovesCount}");
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca

			// --- Rysowanie kolumn Tableau [T1-T7] ---
			Console.WriteLine("Kolumny Gry [T1-T7]:");

			// Znajdź maksymalną wysokość kolumny Tableau
			int maxRows = 0;
			foreach (var tableau in Tableaux) {
				maxRows = Math.Max(maxRows, tableau.Count);
			}

			// Rysuj wiersz po wierszu
			for (int row = -1; row < maxRows; row++) {
				for (int col = 0; col < 7; col++) {
					if (row == -1) { // Nagłówek kolumny
						Console.Write($" T{col + 1}  ");
						continue;
					}

					var currentTableau = Tableaux[col];
					if (row < currentTableau.Count) {
						// Jeśli karta istnieje w tym wierszu i kolumnie
						DisplayCard(currentTableau.GetCardsForDisplay()[row]);
						Console.Write(" "); // Odstęp między kartami w kolumnie
					} else {
						// Puste miejsce, jeśli kolumna jest krótsza
						Console.Write("    "); // 4 spacje dla wyrównania
					}
					Console.Write(" "); // Odstęp między kolumnami
				}
				Console.WriteLine(); // Nowa linia dla następnego wiersza kart
			}
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80)); // Linia oddzielająca
		}

		// Pomocnicza metoda do wyświetlania pojedynczej karty z odpowiednim kolorem
		private void DisplayCard(Card card) {
			if (card.IsFaceUp) {
				Console.ForegroundColor = (card.Color == CardColor.Red) ? ConsoleColor.Red : ConsoleColor.DarkGray; // Ciemnoszary dla czarnych dla lepszej widoczności na niektórych tłach
			} else {
				Console.ForegroundColor = ConsoleColor.White; // Kolor dla zakrytych kart
			}
			Console.Write(card.GetDisplay());
			Console.ResetColor(); // Resetuj kolor po wyświetleniu karty
		}

		// Metoda do obsługi dobierania kart ze stosu rezerwowego (Stock)
		public bool DrawFromStock() {
			// Sprawdź, czy można cofnąć ten ruch (jeśli historia jest pełna)
			bool canUndo = moveHistory.Count < MaxUndoSteps;

			// Jeśli Stock jest pusty, przenieś karty z Waste z powrotem do Stock
			if (Stock.IsEmpty) {
				if (Waste.IsEmpty) {
					Console.WriteLine("Brak kart do dobrania i stos odrzuconych jest pusty.");
					return false; // Nic się nie dzieje
				}

				// Zapisz stan PRZED resetem (jako specjalny ruch "reset")
				// Tworzymy "pseudo" ruch resetu, aby móc go cofnąć
				// Przechowujemy karty z Waste, które zostaną przeniesione
				var wasteCardsBeforeReset = Waste.GetAllCards();
				// Używamy specjalnych indeksów/typów, aby zidentyfikować reset
				var resetRecord = new MoveRecord(PileType.Waste, -1, PileType.Stock, -1, wasteCardsBeforeReset, false, false);
				if (moveHistory.Count >= MaxUndoSteps) moveHistory.Pop(); // Usuń najstarszy ruch, jeśli stos jest pełny
				moveHistory.Push(resetRecord);


				Stock.Reset(Waste.GetAllCards()); // Przenieś karty z Waste
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
				if (moveHistory.Count >= MaxUndoSteps) moveHistory.Pop(); // Usuń najstarszy ruch
				moveHistory.Push(drawRecord);

				Waste.AddCards(drawnCards); // Dodaj dobrane karty do Waste
				MovesCount++; // Dobranie liczy się jako ruch
				return true; // Pomyślnie dobrano karty
			}
			return false; // Nie udało się dobrać (choć to nie powinno się zdarzyć po sprawdzeniu IsEmpty)
		}

		// Metoda do próby przeniesienia karty lub sekwencji kart
		public bool TryMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, int cardCount = 1) {
			CardPile? sourcePile = GetPile(sourceType, sourceIndex);
			CardPile? destPile = GetPile(destType, destIndex);

			if (sourcePile == null || destPile == null || sourcePile == destPile) {
				Console.WriteLine("Błąd: Nieprawidłowe stosy źródłowe lub docelowe.");
				return false;
			}

			List<Card> cardsToMove = new List<Card>();
			bool wasSourceTopFlipped = false;
			bool wasDestFoundationSuitSet = false;

			// --- Logika przenoszenia ---

			// 1. Z Waste do Foundation lub Tableau
			if (sourceType == PileType.Waste) {
				if (cardCount != 1) { Console.WriteLine("Błąd: Można przenieść tylko jedną kartę ze stosu odrzuconych."); return false; }
				Card? card = Waste.GetPlayableCard(); // Zawsze wierzchnia
				if (card == null) { Console.WriteLine("Błąd: Stos odrzuconych jest pusty."); return false; }

				if (destPile.CanAddCard(card)) {
					cardsToMove.Add(card); // Dodajemy kartę do listy przenoszonych
					Waste.RemoveTopCard(); // Usuwamy ją z Waste
				} else {
					Console.WriteLine("Błąd: Nie można przenieść tej karty na wybrany stos docelowy.");
					return false;
				}
			}
			// 2. Z Tableau do Foundation lub innego Tableau
			else if (sourceType == PileType.Tableau) {
				TableauPile sourceTableau = (TableauPile)sourcePile;
				if (sourceTableau.IsEmpty) { Console.WriteLine("Błąd: Kolumna źródłowa jest pusta."); return false; }

				// Sprawdź, czy żądana liczba kart jest poprawna i czy są odkryte
				if (cardCount <= 0 || cardCount > sourceTableau.Count) { Console.WriteLine("Błąd: Nieprawidłowa liczba kart do przeniesienia."); return false; }

				int firstCardIndex = sourceTableau.Count - cardCount;
				if (firstCardIndex < 0) { Console.WriteLine("Błąd: Nie można przenieść tylu kart."); return false; } // Dodatkowe zabezpieczenie

				Card firstCardToMove = sourceTableau.GetCardsForDisplay()[firstCardIndex];
				if (!firstCardToMove.IsFaceUp) { Console.WriteLine("Błąd: Nie można przenieść zakrytej karty lub sekwencji zaczynającej się od zakrytej karty."); return false; }

				// Pobierz sekwencję do przeniesienia
				List<Card> sequence = sourceTableau.GetFaceUpSequence(firstCardIndex);
				if (sequence.Count != cardCount) { Console.WriteLine("Błąd wewnętrzny: Niezgodność liczby kart w sekwencji."); return false; } // Sanity check


				// 2a. Przenoszenie do Foundation (tylko pojedyncza karta)
				if (destType == PileType.Foundation) {
					if (cardCount != 1) { Console.WriteLine("Błąd: Można przenieść tylko jedną kartę na stos końcowy."); return false; }
					if (destPile.CanAddCard(firstCardToMove)) {
						cardsToMove = sourceTableau.RemoveSequence(firstCardIndex); // Usuń kartę z Tableau
																					// Sprawdź, czy trzeba odkryć kartę pod spodem
						wasSourceTopFlipped = sourceTableau.FlipTopCardIfNecessary();
						// Sprawdź, czy ustawiono kolor stosu Foundation
						if (destPile.IsEmpty && firstCardToMove.Rank == Rank.Ace) {
							wasDestFoundationSuitSet = true;
						}
					} else { Console.WriteLine("Błąd: Nie można przenieść tej karty na wybrany stos końcowy."); return false; }
				}
				// 2b. Przenoszenie do innego Tableau
				else if (destType == PileType.Tableau) {
					TableauPile destTableau = (TableauPile)destPile;
					if (destTableau.CanAddSequence(sequence)) // Używamy CanAddSequence
					{
						cardsToMove = sourceTableau.RemoveSequence(firstCardIndex); // Usuń sekwencję z Tableau
																					// Sprawdź, czy trzeba odkryć kartę pod spodem w źródłowym Tableau
						wasSourceTopFlipped = sourceTableau.FlipTopCardIfNecessary();
					} else { Console.WriteLine("Błąd: Nie można przenieść tej sekwencji na wybraną kolumnę."); return false; }
				} else {
					Console.WriteLine("Błąd: Nieprawidłowy cel dla ruchu z kolumny Tableau."); return false;
				}
			}
			// 3. Z Foundation do Tableau (rzadko używane, ale czasem potrzebne)
			else if (sourceType == PileType.Foundation) {
				if (destType != PileType.Tableau) { Console.WriteLine("Błąd: Kartę ze stosu końcowego można przenieść tylko do kolumny gry."); return false; }
				if (cardCount != 1) { Console.WriteLine("Błąd: Można przenieść tylko jedną kartę ze stosu końcowego."); return false; }

				FoundationPile sourceFoundation = (FoundationPile)sourcePile;
				Card? card = sourceFoundation.PeekTopCard();
				if (card == null) { Console.WriteLine("Błąd: Stos końcowy źródłowy jest pusty."); return false; }

				TableauPile destTableau = (TableauPile)destPile;
				if (destTableau.CanAddCard(card)) {
					cardsToMove.Add(sourceFoundation.RemoveTopCard()!); // Usuwamy kartę z Foundation
																		// Sprawdź, czy stos Foundation stał się pusty i trzeba zresetować jego Suit
					if (sourceFoundation.IsEmpty) {
						sourceFoundation.ResetSuitIfEmpty();
						// Uwaga: Cofnięcie tego ruchu musi przywrócić Suit!
						// Potrzebujemy informacji w MoveRecord, że Suit został zresetowany.
						// Na razie uproszczenie: cofnięcie ruchu przywróci kartę i Suit się ustawi.
					}
				} else {
					Console.WriteLine("Błąd: Nie można przenieść tej karty na wybraną kolumnę.");
					return false;
				}
			} else {
				Console.WriteLine("Błąd: Nieprawidłowy stos źródłowy.");
				return false;
			}

			// --- Finalizacja ruchu ---
			if (cardsToMove.Count > 0) {
				// Zapisz ruch do historii PRZED wykonaniem
				var moveRecord = new MoveRecord(sourceType, sourceIndex, destType, destIndex, new List<Card>(cardsToMove), wasSourceTopFlipped, wasDestFoundationSuitSet);
				if (moveHistory.Count >= MaxUndoSteps) moveHistory.Pop(); // Usuń najstarszy ruch
				moveHistory.Push(moveRecord);

				// Dodaj przeniesione karty do stosu docelowego
				destPile.AddCards(cardsToMove);
				MovesCount++; // Zwiększ licznik ruchów
				return true; // Ruch wykonany pomyślnie
			} else {
				// To nie powinno się zdarzyć, jeśli logika powyżej jest poprawna
				Console.WriteLine("Błąd wewnętrzny: Nie znaleziono kart do przeniesienia.");
				return false;
			}
		}

		// Metoda do cofania ostatniego ruchu
		public bool UndoLastMove() {
			if (moveHistory.Count == 0) {
				Console.WriteLine("Brak ruchów do cofnięcia.");
				return false;
			}

			MoveRecord lastMove = moveHistory.Pop(); // Pobierz ostatni ruch ze stosu

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
					moveHistory.Push(lastMove); // Przywróć ruch do historii
					return false;
				}

				// Jeśli cofamy ruch Asa na pusty Foundation, zresetuj Suit tego Foundation
				if (lastMove.DestPileType == PileType.Foundation && lastMove.WasDestFoundationSuitSet) {
					((FoundationPile)destPile).ResetSuitIfEmpty();
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

		// Pomocnicza metoda do porównywania list kart (proste porównanie referencji lub wartości)
		private bool AreCardListsEqual(List<Card> list1, List<Card> list2) {
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
			DisplayGame(); // Pokaż finalny stan planszy
			Console.WriteLine("\n*************************************");
			Console.WriteLine("* Gratulacje! Wygrałeś w Pasjansa! *");
			Console.WriteLine($"* Ukończyłeś grę w {MovesCount} ruchach.    *");
			Console.WriteLine("*************************************\n");

			// Zapisz wynik
			Console.Write("Podaj swoje inicjały (max 3 znaki): ");
			string initials = Console.ReadLine()?.Trim().ToUpper() ?? "XYZ";
			if (initials.Length > 3) initials = initials.Substring(0, 3);
			if (string.IsNullOrWhiteSpace(initials)) initials = "XYZ";

			highScoreManager.AddScore(initials, MovesCount);
			Console.WriteLine("\nRanking najlepszych wyników:");
			highScoreManager.DisplayScores();
			Console.WriteLine("\nNaciśnij Enter, aby zakończyć...");
			Console.ReadLine();
		}

		// Metoda do wyświetlania rankingu
		public void DisplayHighScores() {
			Console.WriteLine("\n--- Ranking Najlepszych Wyników ---");
			highScoreManager.DisplayScores();
			Console.WriteLine("---------------------------------");
		}

		// Metoda głównej pętli gry (przeniesiona z Program.cs)
		public GameResult RunGameLoop() {
			while (true) {
				DisplayGame(); // Wyświetl stan gry

				// Sprawdź warunek zwycięstwa
				if (CheckWinCondition()) {
					HandleWin(); // Obsłuż wygraną
					return GameResult.Continue; // Domyślnie kontynuujemy po wygranej
				}

				// Wyświetl dostępne akcje
				Console.WriteLine("\nAkcje:");
				Console.WriteLine(" - draw / d          : Dobierz kartę ze stosu [S]");
				Console.WriteLine(" - move / m [źr] [cel]: Przenieś kartę/sekwencję (np. m W T2, m T1 F1, m T3 T5 [liczba])");
				Console.WriteLine(" - undo / u          : Cofnij ostatni ruch (do 3 ruchów)");
				Console.WriteLine(" - score / h         : Pokaż ranking");
				Console.WriteLine(" - restart / r       : Rozpocznij nową grę");
				Console.WriteLine(" - quit / q          : Zakończ grę");
				Console.Write("Wybierz akcję: ");

				string? input = Console.ReadLine()?.ToLower().Trim(); // Wczytaj i przetwórz komendę użytkownika

				if (string.IsNullOrWhiteSpace(input)) continue; // Ignoruj puste linie

				string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries); // Podziel komendę na części
				string command = parts[0];

				try { // Obsługa potencjalnych błędów parsowania komend
					switch (command) {
						case "draw":
						case "d":
							if (!DrawFromStock()) {
								Console.WriteLine("Nie można dobrać karty.");
								Pause();
							}
							break;

						case "move":
						case "m":
							if (parts.Length < 3) {
								Console.WriteLine("Nieprawidłowa komenda 'move'. Użycie: move [źródło] [cel] [liczba_kart - opcjonalnie]");
								Console.WriteLine("Źródła: S (Stock - nie można), W (Waste), F1-F4 (Foundation), T1-T7 (Tableau)");
								Console.WriteLine("Cele: F1-F4, T1-T7");
								Pause();
								continue;
							}
							string sourceStr = parts[1].ToUpper();
							string destStr = parts[2].ToUpper();
							int cardCount = 1; // Domyślnie przenosimy 1 kartę

							// Sprawdź, czy podano liczbę kart (dla ruchu T->T)
							if (parts.Length > 3) {
								if (!int.TryParse(parts[3], out cardCount) || cardCount < 1) {
									Console.WriteLine("Nieprawidłowa liczba kart. Musi być dodatnią liczbą całkowitą.");
									Pause();
									continue;
								}
							}

							// Parsowanie źródła
							PileType sourceType;
							int sourceIndex = ParsePileString(sourceStr, out sourceType);
							if (sourceIndex == -1 || sourceType == PileType.Stock) // Nie można ruszać ze Stock bezpośrednio
							{
								Console.WriteLine($"Nieprawidłowe źródło: {sourceStr}"); Pause(); continue;
							}

							// Parsowanie celu
							PileType destType;
							int destIndex = ParsePileString(destStr, out destType);
							if (destIndex == -1 || destType == PileType.Stock || destType == PileType.Waste) // Nie można ruszać na Stock ani Waste
							{
								Console.WriteLine($"Nieprawidłowy cel: {destStr}"); Pause(); continue;
							}

							// Wykonaj ruch
							if (!TryMove(sourceType, sourceIndex, destType, destIndex, cardCount)) {
								// Komunikat o błędzie jest już wyświetlany w TryMove
								Pause();
							}
							break;

						case "undo":
						case "u":
							if (!UndoLastMove()) {
								// Komunikat o błędzie jest już wyświetlany w UndoLastMove
								Pause();
							}
							break;

						case "score":
						case "h":
							Console.Clear();
							DisplayHighScores();
							Console.WriteLine("\nNaciśnij Enter, aby wrócić do gry...");
							Console.ReadLine();
							break;

						case "restart":
						case "r":
							Console.Write("Czy na pewno chcesz rozpocząć nową grę? (t/n): ");
							if (Console.ReadLine()?.ToLower() == "t") {
								return GameResult.Restart; // Sygnalizuj chęć rozpoczęcia nowej gry
							}
							break;

						case "quit":
						case "q":
							Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
							if (Console.ReadLine()?.ToLower() == "t") {
								return GameResult.Quit; // Sygnalizuj chęć zakończenia gry
							}
							break;

						default:
							Console.WriteLine("Nieznana komenda.");
							Pause();
							break;
					}
				} catch (Exception ex) {
					Console.WriteLine($"\nWystąpił nieoczekiwany błąd: {ex.Message}");
					Console.WriteLine("Spróbuj ponownie lub uruchom grę od nowa.");
					Pause();
				}
			}
		}

		// Pomocnicza metoda do parsowania stringa reprezentującego stos (np. "T1", "F3", "W")
		// Zwraca indeks stosu (0-based) i ustawia typ stosu przez parametr 'out'
		// Zwraca -1 w przypadku błędu.
		private int ParsePileString(string pileStr, out PileType type) {
			type = PileType.Stock; // Domyślna wartość na wypadek błędu

			if (string.IsNullOrEmpty(pileStr)) return -1;

			char pileChar = pileStr[0];
			string indexStr = pileStr.Length > 1 ? pileStr.Substring(1) : "";
			int index = 0; // Domyślny indeks dla W

			switch (pileChar) {
				case 'W': // Waste Pile
					if (pileStr.Length > 1) return -1; // Waste nie ma indeksu (W, nie W1)
					type = PileType.Waste;
					return 0; // Zwracamy 0 jako placeholder, bo Waste jest tylko jedno

				case 'F': // Foundation Pile
					if (!int.TryParse(indexStr, out index) || index < 1 || index > 4) return -1; // Indeksy F1-F4
					type = PileType.Foundation;
					return index - 1; // Zwracamy indeks 0-based (0-3)

				case 'T': // Tableau Pile
					if (!int.TryParse(indexStr, out index) || index < 1 || index > 7) return -1; // Indeksy T1-T7
					type = PileType.Tableau;
					return index - 1; // Zwracamy indeks 0-based (0-6)

				default:
					return -1; // Nieznany typ stosu
			}
		}

		// Prosta metoda pauzująca grę do czasu naciśnięcia Enter
		private void Pause() {
			Console.WriteLine("\nNaciśnij Enter, aby kontynuować...");
			Console.ReadLine();
		}
	}
}