using System.Text; // Potrzebne dla StringBuilder

namespace SolitaireConsole {
	// Enum reprezentujący kolory kart (Czerwony/Czarny)
	public enum CardColor {
		Red,
		Black
	}

	// Enum reprezentujący kolory kart (kier, karo, pik, trefl)
	public enum Suit {
		Hearts,   // Kier ♥ (Czerwony)
		Diamonds, // Karo ♦ (Czerwony)
		Clubs,    // Trefl ♣ (Czarny)
		Spades    // Pik ♠ (Czarny)
	}

	// Enum reprezentujący figury kart (As, 2-10, Walet, Dama, Król)
	public enum Rank {
		Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
	}

	// Klasa reprezentująca pojedynczą kartę
	public class Card {
		public Suit Suit { get; } // Kolor karty (kier, karo, pik, trefl)
		public Rank Rank { get; } // Figura karty (As-Król)
		public bool IsFaceUp { get; set; } // Czy karta jest odkryta?

		// Konstruktor karty
		public Card(Suit suit, Rank rank) {
			Suit = suit;
			Rank = rank;
			IsFaceUp = false; // Domyślnie karta jest zakryta
		}

		// Zwraca kolor karty (Czerwony/Czarny) na podstawie jej koloru (Suit)
		public CardColor Color => (Suit == Suit.Hearts || Suit == Suit.Diamonds) ? CardColor.Red : CardColor.Black;

		// Zwraca reprezentację tekstową karty (np. " A♥", "10♠", " K♦")
		// lub "[ ]" jeśli zakryta
		public string GetDisplay() {
			if (!IsFaceUp) {
				return "[ ]"; // Reprezentacja zakrytej karty
			}

			string rankString;
			switch (Rank) {
				case Rank.Ace: rankString = "A"; break;
				case Rank.Jack: rankString = "J"; break;
				case Rank.Queen: rankString = "Q"; break;
				case Rank.King: rankString = "K"; break;
				case Rank.Ten: rankString = "T"; break; // Używamy T dla 10 dla spójnej szerokości
				default: rankString = ((int)Rank).ToString(); break;
			}

			char suitChar;
			switch (Suit) {
				case Suit.Hearts: suitChar = '♥'; break;
				case Suit.Diamonds: suitChar = '♦'; break;
				case Suit.Clubs: suitChar = '♣'; break;
				case Suit.Spades: suitChar = '♠'; break;
				default: suitChar = '?'; break; // Na wypadek błędu
			}

			// Dodajemy spację dla jednocyfrowych rang dla wyrównania
			return $"{(rankString.Length == 1 ? " " : "")}{rankString}{suitChar}";
		}

		// Nadpisanie metody ToString dla łatwiejszego debugowania
		public override string ToString() {
			return $"{Rank} of {Suit}";
		}
	}

	// Klasa reprezentująca talię kart
	public class Deck {
		private List<Card> cards; // Lista kart w talii
		private static readonly Random rng = new Random(); // Generator liczb losowych do tasowania

		// Konstruktor tworzący pełną, potasowaną talię 52 kart
		public Deck() {
			cards = new List<Card>();
			// Pętla po wszystkich kolorach (Suit)
			foreach (Suit s in Enum.GetValues(typeof(Suit))) {
				// Pętla po wszystkich figurach (Rank)
				foreach (Rank r in Enum.GetValues(typeof(Rank))) {
					cards.Add(new Card(s, r)); // Dodanie nowej karty do listy
				}
			}
			Shuffle(); // Potasowanie talii po utworzeniu
		}

		// Metoda tasująca karty w talii (algorytm Fisher-Yates)
		public void Shuffle() {
			int n = cards.Count;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1); // Losowy indeks
				Card value = cards[k]; // Zamiana miejscami kart
				cards[k] = cards[n];
				cards[n] = value;
			}
		}

		// Metoda dobierająca jedną kartę z wierzchu talii
		// Zwraca null, jeśli talia jest pusta
		public Card? Deal() {
			if (cards.Count == 0) {
				return null; // Brak kart do dobrania
			}
			Card card = cards[cards.Count - 1]; // Pobranie ostatniej karty z listy
			cards.RemoveAt(cards.Count - 1); // Usunięcie karty z listy
			return card;
		}

		// Zwraca liczbę kart pozostałych w talii
		public int Count => cards.Count;

		// Metoda dodająca listę kart z powrotem do talii (np. z WastePile)
		public void AddCards(IEnumerable<Card> cardsToAdd) {
			foreach (var card in cardsToAdd) {
				card.IsFaceUp = false; // Zakrywamy karty przed dodaniem do talii
				cards.Add(card);
			}
		}
	}

	// Klasa bazowa dla różnych stosów kart (abstrakcyjna)
	public abstract class CardPile {
		protected List<Card> cards;
		public abstract PileType PileType { get; } // Dodana właściwość abstrakcyjna

		protected CardPile() { cards = new List<Card>(); }
		public int Count => cards.Count;
		public bool IsEmpty => cards.Count == 0;
		public virtual void AddCard(Card card) { cards.Add(card); }
		public virtual void AddCards(IEnumerable<Card> cardsToAdd) { cards.AddRange(cardsToAdd); }
		public Card? PeekTopCard() { return cards.LastOrDefault(); }
		public virtual Card? RemoveTopCard() {
			if (IsEmpty) return null;
			Card card = cards[cards.Count - 1];
			cards.RemoveAt(cards.Count - 1);
			return card;
		}
		public virtual List<Card> RemoveTopCards(int count) {
			if (count <= 0 || count > cards.Count) return new List<Card>();
			List<Card> removed = cards.GetRange(cards.Count - count, count);
			cards.RemoveRange(cards.Count - count, count);
			return removed;
		}
		public List<Card> GetAllCards() { return new List<Card>(cards); }
		public void Clear() { cards.Clear(); }
		public abstract bool CanAddCard(Card card); // Pozostaje abstrakcyjna
		public Card? GetCardAt(int index) // Pomocnicza metoda do pobrania karty na danej pozycji
		{
			if (index >= 0 && index < cards.Count) return cards[index];
			return null;
		}
	}
	// Stos rezerwowy (Stock) - skąd dobieramy karty
	public class StockPile : CardPile {
		private Deck deck;
		public override PileType PileType => PileType.Stock; // Implementacja właściwości
		public StockPile() {
			deck = new Deck();
			while (deck.Count > 0) { Card? card = deck.Deal(); if (card != null) { card.IsFaceUp = false; AddCard(card); } }
		}
		public List<Card> DealInitialTableauCards(int count) { return RemoveTopCards(count); }
		public List<Card> Draw(int drawCount) {
			int actualDrawCount = Math.Min(drawCount, cards.Count);
			List<Card> drawnCards = RemoveTopCards(actualDrawCount);
			foreach (var card in drawnCards) card.IsFaceUp = true;
			return drawnCards;
		}
		public void Reset(IEnumerable<Card> wasteCards) {
			AddCards(wasteCards.Reverse());
			ShuffleStock();
		}
		private void ShuffleStock() {
			int n = cards.Count; var rng = new Random();
			while (n > 1) { n--; int k = rng.Next(n + 1); Card value = cards[k]; cards[k] = cards[n]; cards[n] = value; }
			foreach (var card in cards) card.IsFaceUp = false;
		}
		public override bool CanAddCard(Card card) => false;
	}
	// Stos kart odrzuconych (Waste) - gdzie trafiają karty ze Stock
	public class WastePile : CardPile {
		public override PileType PileType => PileType.Waste; // Implementacja właściwości
		public WastePile() : base() { }
		public override void AddCards(IEnumerable<Card> cardsToAdd) {
			foreach (var card in cardsToAdd) card.IsFaceUp = true;
			base.AddCards(cardsToAdd);
		}
		public Card? GetPlayableCard() => PeekTopCard();
		public override Card? RemoveTopCard() => base.RemoveTopCard();
		public override bool CanAddCard(Card card) => false;
		// Zwraca karty widoczne na stosie Waste (1 lub 3)
		public List<Card> GetVisibleCards(bool isHardMode) {
			if (IsEmpty) return new List<Card>();
			int count = isHardMode ? 3 : 1;
			int startIndex = Math.Max(0, cards.Count - count);
			return cards.GetRange(startIndex, cards.Count - startIndex);
		}
	}

	// Stos końcowy (Foundation) - gdzie układamy karty od Asa do Króla
	public class FoundationPile : CardPile {
		public Suit PileSuit { get; private set; }
		private bool suitSet = false;
		public override PileType PileType => PileType.Foundation; // Implementacja właściwości
		public FoundationPile() : base() { }
		public override bool CanAddCard(Card card) {
			if (IsEmpty) return card.Rank == Rank.Ace;
			else { Card? topCard = PeekTopCard(); return topCard != null && card.Suit == topCard.Suit && card.Rank == topCard.Rank + 1; }
		}
		public override void AddCard(Card card) {
			if (CanAddCard(card)) {
				if (IsEmpty) { PileSuit = card.Suit; suitSet = true; }
				card.IsFaceUp = true;
				base.AddCard(card);
			}
		}
		public void ResetSuitIfEmpty() { if (IsEmpty) suitSet = false; }
		public Suit? GetPileSuit() => suitSet ? PileSuit : (Suit?)null;
	}

	// Kolumna gry (Tableau) - 7 kolumn na planszy
	public class TableauPile : CardPile {
		public override PileType PileType => PileType.Tableau; // Implementacja właściwości
		public TableauPile() : base() { }
		public void DealInitialCards(List<Card> initialCards) {
			cards.AddRange(initialCards);
			if (cards.Count > 0) cards.Last().IsFaceUp = true;
		}
		public override bool CanAddCard(Card card) {
			if (IsEmpty) return card.Rank == Rank.King;
			else { Card? topCard = PeekTopCard(); return topCard != null && topCard.IsFaceUp && card.Color != topCard.Color && card.Rank == topCard.Rank - 1; }
		}
		public bool CanAddSequence(List<Card> sequence) {
			if (sequence == null || sequence.Count == 0 || !sequence.First().IsFaceUp) return false;
			return CanAddCard(sequence.First());
		}
		public bool FlipTopCardIfNecessary() {
			if (!IsEmpty && !cards.Last().IsFaceUp) { cards.Last().IsFaceUp = true; return true; }
			return false;
		}
		public int FindFirstFaceUpCardIndex() {
			for (int i = 0; i < cards.Count; i++) if (cards[i].IsFaceUp) return i;
			return -1;
		}
		// Zwraca sekwencję kart od podanego indeksu (jeśli są odkryte)
		public List<Card> GetSequenceFromIndex(int startIndex) {
			if (startIndex < 0 || startIndex >= cards.Count || !cards[startIndex].IsFaceUp) return new List<Card>();
			return cards.GetRange(startIndex, cards.Count - startIndex);
		}
		public List<Card> RemoveSequence(int startIndex) {
			if (startIndex < 0 || startIndex >= cards.Count) return new List<Card>();
			int count = cards.Count - startIndex;
			List<Card> removed = cards.GetRange(startIndex, count);
			cards.RemoveRange(startIndex, count);
			return removed;
		}
		public List<Card> GetCardsForDisplay() => cards;
	}
	// Enum określający typ stosu (do identyfikacji w ruchach)
	public enum PileType { Stock, Waste, Foundation, Tableau }

	// Klasa reprezentująca pojedynczy ruch (do mechanizmu Undo)
	public class MoveRecord {
		public PileType SourcePileType { get; }
		public int SourcePileIndex { get; } // Indeks dla Tableau/Foundation (0 dla Stock/Waste)
		public PileType DestPileType { get; }
		public int DestPileIndex { get; }   // Indeks dla Tableau/Foundation (0 dla Stock/Waste)
		public List<Card> MovedCards { get; } // Przeniesione karty
		public bool WasSourceTopCardFlipped { get; } // Czy odkryto kartę na stosie źródłowym po ruchu?
		public bool WasDestFoundationSuitSet { get; } // Czy ustawiono kolor stosu Foundation (przy ruchu Asa)?

		// Konstruktor
		public MoveRecord(PileType sourceType, int sourceIndex, PileType destType, int destIndex, List<Card> movedCards, bool flipped, bool foundationSet) {
			SourcePileType = sourceType;
			SourcePileIndex = sourceIndex;
			DestPileType = destType;
			DestPileIndex = destIndex;
			MovedCards = movedCards; // Przechowujemy kopię listy
			WasSourceTopCardFlipped = flipped;
			WasDestFoundationSuitSet = foundationSet;
		}
	}

	// Główna klasa zarządzająca logiką gry
	public class Game {
		public StockPile Stock { get; private set; }
		public WastePile Waste { get; private set; }
		public List<FoundationPile> Foundations { get; private set; }
		public List<TableauPile> Tableaux { get; private set; }
		public bool IsHardMode { get; private set; }
		public int MovesCount { get; private set; }
		private const int MaxUndoSteps = 3;
		private Stack<MoveRecord> moveHistory;
		private HighScoreManager highScoreManager;

		public Game(bool hardMode) {
			IsHardMode = hardMode;
			Stock = new StockPile();
			Waste = new WastePile();
			Foundations = new List<FoundationPile>(4);
			Tableaux = new List<TableauPile>(7);
			moveHistory = new Stack<MoveRecord>();
			MovesCount = 0;
			highScoreManager = new HighScoreManager("highscores.txt");
			for (int i = 0; i < 4; i++) Foundations.Add(new FoundationPile());
			for (int i = 0; i < 7; i++) Tableaux.Add(new TableauPile());
			for (int i = 0; i < 7; i++) {
				List<Card> cardsToDeal = Stock.DealInitialTableauCards(i + 1);
				Tableaux[i].DealInitialCards(cardsToDeal);
			}
		}

		// Metoda DisplayGame została przeniesiona i zmodyfikowana w klasie Program
		// (bo teraz zawiera logikę kursora)

		public bool DrawFromStock() {
			bool canUndo = moveHistory.Count < MaxUndoSteps;
			if (Stock.IsEmpty) {
				if (Waste.IsEmpty) return false;
				var wasteCardsBeforeReset = Waste.GetAllCards();
				var resetRecord = new MoveRecord(PileType.Waste, -1, PileType.Stock, -1, wasteCardsBeforeReset, false, false);
				if (moveHistory.Count >= MaxUndoSteps) moveHistory.TryPop(out _); // Użyj TryPop
				moveHistory.Push(resetRecord);
				Stock.Reset(Waste.GetAllCards());
				Waste.Clear();
				MovesCount++;
				return true;
			}
			int cardsToDraw = IsHardMode ? 3 : 1;
			List<Card> drawnCards = Stock.Draw(cardsToDraw);
			if (drawnCards.Count > 0) {
				var drawRecord = new MoveRecord(PileType.Stock, 0, PileType.Waste, 0, drawnCards, false, false);
				if (moveHistory.Count >= MaxUndoSteps) moveHistory.TryPop(out _);
				moveHistory.Push(drawRecord);
				Waste.AddCards(drawnCards);
				MovesCount++;
				return true;
			}
			return false;
		}

		// TryMove pozostaje bardzo podobne, ale zwraca bool zamiast pisać do konsoli
		public bool TryMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, int cardCount = 1) {
			CardPile? sourcePile = GetPile(sourceType, sourceIndex);
			CardPile? destPile = GetPile(destType, destIndex);

			if (sourcePile == null || destPile == null || sourcePile == destPile) return false;


			List<Card> cardsToMove = new List<Card>();
			bool wasSourceTopFlipped = false;
			bool wasDestFoundationSuitSet = false;

			// --- Logika przenoszenia (uproszczona - bez Console.WriteLine) ---

			// 1. Z Waste
			if (sourceType == PileType.Waste) {
				if (cardCount != 1) return false;
				Card? card = Waste.GetPlayableCard();
				if (card == null) return false;
				if (destPile.CanAddCard(card)) {
					cardsToMove.Add(card);
					Waste.RemoveTopCard();
				} else return false;
			}
			// 2. Z Tableau
			else if (sourceType == PileType.Tableau) {
				TableauPile sourceTableau = (TableauPile)sourcePile;
				if (sourceTableau.IsEmpty || cardCount <= 0 || cardCount > sourceTableau.Count) return false;

				int firstCardIndex = sourceTableau.Count - cardCount;
				if (firstCardIndex < 0) return false;

				Card? firstCardToMove = sourceTableau.GetCardAt(firstCardIndex);
				if (firstCardToMove == null || !firstCardToMove.IsFaceUp) return false;

				List<Card> sequence = sourceTableau.GetSequenceFromIndex(firstCardIndex);
				if (sequence.Count != cardCount) return false; // Sanity check

				// 2a. Do Foundation
				if (destType == PileType.Foundation) {
					if (cardCount != 1) return false;
					if (destPile.CanAddCard(firstCardToMove)) {
						cardsToMove = sourceTableau.RemoveSequence(firstCardIndex);
						wasSourceTopFlipped = sourceTableau.FlipTopCardIfNecessary();
						if (destPile.IsEmpty && firstCardToMove.Rank == Rank.Ace) wasDestFoundationSuitSet = true;
					} else return false;
				}
				// 2b. Do innego Tableau
				else if (destType == PileType.Tableau) {
					TableauPile destTableau = (TableauPile)destPile;
					if (destTableau.CanAddSequence(sequence)) {
						cardsToMove = sourceTableau.RemoveSequence(firstCardIndex);
						wasSourceTopFlipped = sourceTableau.FlipTopCardIfNecessary();
					} else return false;
				} else return false; // Nieprawidłowy cel
			}
			// 3. Z Foundation
			else if (sourceType == PileType.Foundation) {
				if (destType != PileType.Tableau || cardCount != 1) return false;
				FoundationPile sourceFoundation = (FoundationPile)sourcePile;
				Card? card = sourceFoundation.PeekTopCard();
				if (card == null) return false;
				TableauPile destTableau = (TableauPile)destPile;
				if (destTableau.CanAddCard(card)) {
					cardsToMove.Add(sourceFoundation.RemoveTopCard()!);
					if (sourceFoundation.IsEmpty) sourceFoundation.ResetSuitIfEmpty();
				} else return false;
			} else return false; // Nieprawidłowe źródło

			// --- Finalizacja ---
			if (cardsToMove.Count > 0) {
				var moveRecord = new MoveRecord(sourceType, sourceIndex, destType, destIndex, new List<Card>(cardsToMove), wasSourceTopFlipped, wasDestFoundationSuitSet);
				if (moveHistory.Count >= MaxUndoSteps) moveHistory.TryPop(out _);
				moveHistory.Push(moveRecord);
				destPile.AddCards(cardsToMove);
				MovesCount++;
				return true;
			}
			return false;
		}

		// UndoLastMove pozostaje bardzo podobne, ale zwraca bool
		public bool UndoLastMove() {
			if (moveHistory.Count == 0) return false;

			MoveRecord lastMove = moveHistory.Pop();

			// Cofanie resetu Stock/Waste
			if (lastMove.SourcePileType == PileType.Waste && lastMove.SourcePileIndex == -1 && lastMove.DestPileType == PileType.Stock) {
				Stock.Clear(); Waste.Clear(); Waste.AddCards(lastMove.MovedCards); MovesCount--; return true;
			}
			// Cofanie dobrania kart (Stock -> Waste)
			else if (lastMove.SourcePileType == PileType.Stock && lastMove.DestPileType == PileType.Waste) {
				List<Card> cardsToReturn = new List<Card>();
				for (int i = 0; i < lastMove.MovedCards.Count; ++i) { Card? card = Waste.RemoveTopCard(); if (card != null) cardsToReturn.Add(card); }
				cardsToReturn.Reverse();
				foreach (var card in cardsToReturn) { card.IsFaceUp = false; Stock.AddCard(card); }
				MovesCount--; return true;
			}
			// Standardowy ruch
			else {
				CardPile? sourcePile = GetPile(lastMove.SourcePileType, lastMove.SourcePileIndex);
				CardPile? destPile = GetPile(lastMove.DestPileType, lastMove.DestPileIndex);
				if (sourcePile == null || destPile == null) { moveHistory.Push(lastMove); return false; } // Błąd -> przywróć ruch

				List<Card> removedFromDest = destPile.RemoveTopCards(lastMove.MovedCards.Count);
				if (!AreCardListsEqual(removedFromDest, lastMove.MovedCards)) {
					destPile.AddCards(removedFromDest); moveHistory.Push(lastMove); return false; // Błąd -> przywróć
				}

				if (lastMove.DestPileType == PileType.Foundation && lastMove.WasDestFoundationSuitSet) {
					((FoundationPile)destPile).ResetSuitIfEmpty();
				}

				if (lastMove.WasSourceTopCardFlipped && lastMove.SourcePileType == PileType.Tableau) {
					Card? topCard = sourcePile.PeekTopCard();
					if (topCard != null) topCard.IsFaceUp = false;
				}

				sourcePile.AddCards(lastMove.MovedCards);
				MovesCount--;
				return true;
			}
		}

		private bool AreCardListsEqual(List<Card> list1, List<Card> list2) {
			if (list1.Count != list2.Count) return false;
			for (int i = 0; i < list1.Count; i++) if (list1[i].Suit != list2[i].Suit || list1[i].Rank != list2[i].Rank) return false;
			return true;
		}

		public CardPile? GetPile(PileType type, int index) {
			try {
				switch (type) {
					case PileType.Stock: return Stock;
					case PileType.Waste: return Waste;
					case PileType.Foundation: return Foundations[index];
					case PileType.Tableau: return Tableaux[index];
					default: return null;
				}
			} catch { return null; }
		}
		public bool CheckWinCondition() => Foundations.All(f => f.Count == 13);
		public void HandleWin() {
			// Wyświetlanie musi być teraz zrobione w Program.Main
			Console.Clear();
			Console.WriteLine("\n*************************************");
			Console.WriteLine("* Gratulacje! Wygrałeś w Pasjansa! *");
			Console.WriteLine($"* Ukończyłeś grę w {MovesCount} ruchach.    *");
			Console.WriteLine("*************************************\n");
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
		public void DisplayHighScores() {
			Console.WriteLine("\n--- Ranking Najlepszych Wyników ---");
			highScoreManager.DisplayScores();
			Console.WriteLine("---------------------------------");
		}
	}

	// Klasa do zarządzania najlepszymi wynikami
	public class HighScoreManager {
		private readonly string filePath; // Ścieżka do pliku z wynikami
		private List<(string Name, int Score)> highScores; // Lista wyników (Imię, Liczba ruchów)

		// Konstruktor
		public HighScoreManager(string fileName) {
			filePath = Path.Combine(Environment.CurrentDirectory, fileName); // Zapisuje w katalogu aplikacji
			highScores = LoadScores();
		}

		// Wczytuje wyniki z pliku
		private List<(string Name, int Score)> LoadScores() {
			var scores = new List<(string Name, int Score)>();
			if (!File.Exists(filePath)) {
				return scores; // Zwraca pustą listę, jeśli plik nie istnieje
			}

			try {
				string[] lines = File.ReadAllLines(filePath);
				foreach (string line in lines) {
					string[] parts = line.Split(','); // Zakładamy format: INICJAŁY,WYNIK
					if (parts.Length == 2 && int.TryParse(parts[1], out int score)) {
						scores.Add((parts[0].Trim(), score));
					}
				}
			} catch (Exception ex) {
				Console.WriteLine($"Błąd podczas wczytywania rankingu: {ex.Message}");
				// Kontynuuje z pustą listą lub tym, co udało się wczytać
			}

			// Sortuje wyniki (im mniej ruchów, tym lepiej)
			scores.Sort((a, b) => a.Score.CompareTo(b.Score));
			return scores;
		}

		// Dodaje nowy wynik i zapisuje do pliku
		public void AddScore(string name, int score) {
			highScores.Add((name, score));
			// Sortuje ponownie po dodaniu nowego wyniku
			highScores.Sort((a, b) => a.Score.CompareTo(b.Score));
			// Opcjonalnie: ogranicz liczbę zapisanych wyników, np. do Top 10
			// if (highScores.Count > 10) highScores = highScores.Take(10).ToList();

			SaveScores(); // Zapisuje zaktualizowaną listę do pliku
		}

		// Zapisuje wyniki do pliku
		private void SaveScores() {
			try {
				// Tworzy listę linii w formacie "INICJAŁY,WYNIK"
				var lines = highScores.Select(score => $"{score.Name},{score.Score}");
				File.WriteAllLines(filePath, lines); // Nadpisuje plik nowymi wynikami
			} catch (Exception ex) {
				Console.WriteLine($"Błąd podczas zapisywania rankingu: {ex.Message}");
			}
		}

		// Wyświetla ranking w konsoli
		public void DisplayScores() {
			if (highScores.Count == 0) {
				Console.WriteLine("Brak zapisanych wyników.");
				return;
			}

			Console.WriteLine(" # | Inicjały | Ruchy");
			Console.WriteLine("---|----------|-------");
			int rank = 1;
			foreach (var score in highScores) {
				Console.WriteLine($"{rank,2} | {score.Name,-8} | {score.Score}");
				rank++;
			}
		}
	}

	public enum ActiveArea { Stock, Waste, Foundation, Tableau }

	// Główna klasa programu
	class Program {
		// Stan UI
		private static ActiveArea currentArea = ActiveArea.Stock; // Aktualnie aktywny obszar
		private static int currentPileIndex = 0; // Indeks stosu w Foundation (0-3) lub Tableau (0-6)
		private static int currentCardIndex = 0; // Indeks karty w Tableau (licząc od góry, 0 = najwyższa)

		private static ActiveArea selectedArea = ActiveArea.Stock; // Obszar zaznaczonego źródła
		private static int selectedPileIndex = -1; // Indeks zaznaczonego stosu źródłowego (-1 = nic nie zaznaczono)
		private static int selectedCardIndex = -1; // Indeks zaznaczonej karty źródłowej w Tableau (-1 = cały stos/nie dotyczy)
		private static int selectedCardCount = 0; // Liczba zaznaczonych kart w sekwencji

		private static string message = ""; // Komunikat dla użytkownika

		static void Main(string[] args) {
			Console.Title = "Pasjans Konsolowy (Sterowanie Strzałkami)";
			Console.OutputEncoding = Encoding.UTF8;
			Console.CursorVisible = false; // Ukryj kursor systemowy

			bool playAgain = true;
			while (playAgain) {
				bool? hardMode = ChooseDifficulty();
				if (!hardMode.HasValue) { playAgain = false; continue; }

				Game game = new Game(hardMode.Value);
				ResetSelection(); // Zresetuj zaznaczenie na start gry

				// Główna pętla gry
				while (true) {
					DisplayGame(game); // Wyświetl stan gry z kursorem/zaznaczeniem

					if (game.CheckWinCondition()) {
						game.HandleWin(); // Obsłuż wygraną (to czyści konsolę i czeka na Enter)
						break; // Zakończ pętlę gry
					}

					// Odczytaj klawisz
					ConsoleKeyInfo keyInfo = Console.ReadKey(true); // true = nie wyświetlaj wciśniętego klawisza

					message = ""; // Wyczyść komunikat przed obsługą nowego klawisza

					// Obsługa klawiszy
					switch (keyInfo.Key) {
						// --- Nawigacja ---
						case ConsoleKey.UpArrow: MoveCursor(game, 0, -1); break;
						case ConsoleKey.DownArrow: MoveCursor(game, 0, 1); break;
						case ConsoleKey.LeftArrow: MoveCursor(game, -1, 0); break;
						case ConsoleKey.RightArrow: MoveCursor(game, 1, 0); break;

						// --- Akcje ---
						case ConsoleKey.Enter:
						case ConsoleKey.Spacebar:
							HandleSelectionOrMove(game);
							break;
						case ConsoleKey.D: // Dobierz ze Stock
							if (selectedPileIndex != -1) { message = "Najpierw zakończ lub anuluj zaznaczenie (Esc)."; break; }
							if (!game.DrawFromStock()) message = "Nie można dobrać karty.";
							// Po dobraniu, automatycznie ustaw kursor na Waste, jeśli to możliwe
							if (game.Waste.Count > 0) { currentArea = ActiveArea.Waste; currentPileIndex = 0; currentCardIndex = 0; }
							break;
						case ConsoleKey.U: // Undo
							if (selectedPileIndex != -1) { message = "Najpierw zakończ lub anuluj zaznaczenie (Esc)."; break; }
							if (!game.UndoLastMove()) message = "Brak ruchów do cofnięcia.";
							else message = "Cofnięto ruch.";
							break;
						case ConsoleKey.Escape: // Anuluj zaznaczenie
							if (selectedPileIndex != -1) {
								ResetSelection();
								message = "Anulowano zaznaczenie.";
							}
							break;

						// --- Inne ---
						case ConsoleKey.H: // High Scores
							if (selectedPileIndex != -1) { message = "Najpierw zakończ lub anuluj zaznaczenie (Esc)."; break; }
							Console.Clear();
							game.DisplayHighScores();
							Console.WriteLine("\nNaciśnij Enter, aby wrócić do gry...");
							Console.ReadLine();
							break;
						case ConsoleKey.R: // Restart
							if (selectedPileIndex != -1) { message = "Najpierw zakończ lub anuluj zaznaczenie (Esc)."; break; }
							Console.Write("Czy na pewno chcesz rozpocząć nową grę? (t/n): ");
							if (Console.ReadKey().KeyChar == 't') goto NewGame; // Użycie goto
							message = "Anulowano restart."; // Jeśli nie 't'
							break;
						case ConsoleKey.Q: // Quit
							if (selectedPileIndex != -1) { message = "Najpierw zakończ lub anuluj zaznaczenie (Esc)."; break; }
							Console.Write("Czy na pewno chcesz zakończyć grę? (t/n): ");
							if (Console.ReadKey().KeyChar == 't') { playAgain = false; goto EndGame; } // Użycie goto
							message = "Anulowano wyjście."; // Jeśli nie 't'
							break;
					}
				} // Koniec pętli gry (while true)

			EndGame:; // Etykieta dla wyjścia z pętli gry
			NewGame:; // Etykieta dla rozpoczęcia nowej gry
				if (playAgain) {
					Console.Clear(); // Wyczyść przed komunikatem o nowej grze
					Console.WriteLine("\nRozpoczynanie nowej gry...");
					System.Threading.Thread.Sleep(1000);
				}
			} // Koniec pętli ponownego grania (while playAgain)

			Console.CursorVisible = true; // Przywróć kursor systemowy
			Console.Clear();
			Console.WriteLine("\nDziękujemy za grę! Do zobaczenia!");
		}

		// Metoda do rysowania stanu gry z uwzględnieniem kursora i zaznaczenia
		static void DisplayGame(Game game) {
			Console.Clear();
			Console.OutputEncoding = Encoding.UTF8;

			// --- Górny rząd: Stock, Waste, Foundations ---
			Console.Write("Stos [S]: ");
			DisplayElement(ActiveArea.Stock, 0, 0, game.Stock.IsEmpty ? "[   ]" : "[ * ]", $"({game.Stock.Count})");

			Console.Write("  Odrzucone [W]: ");
			var visibleWaste = game.Waste.GetVisibleCards(game.IsHardMode);
			if (visibleWaste.Count == 0) {
				DisplayElement(ActiveArea.Waste, 0, 0, "[   ]", "");
			} else {
				// W Waste zawsze zaznaczamy/kursoryzujemy ostatnią kartę
				for (int i = 0; i < visibleWaste.Count; i++) {
					bool isLast = (i == visibleWaste.Count - 1);
					DisplayElement(ActiveArea.Waste, 0, 0, visibleWaste[i].GetDisplay(), isLast ? "" : " ", isLast); // Tylko ostatnia jest "targetable"
					if (!isLast) Console.Write(" "); // Dodaj spację między kartami w Waste (jeśli są 3)
				}
				// Dopełnienie spacjami dla trybu trudnego, jeśli mniej niż 3 karty
				if (game.IsHardMode && visibleWaste.Count < 3) {
					for (int i = 0; i < 3 - visibleWaste.Count; i++) Console.Write("    "); // 4 spacje na kartę
				}
			}

			Console.Write("    Stosy Końcowe [F1-F4]: ");
			for (int i = 0; i < 4; i++) {
				Card? topCard = game.Foundations[i].PeekTopCard();
				string display = "[   ]";
				if (topCard != null) display = topCard.GetDisplay();
				else {
					Suit? pileSuit = game.Foundations[i].GetPileSuit();
					if (pileSuit.HasValue) {
						char suitChar = '?'; ConsoleColor color = ConsoleColor.White;
						switch (pileSuit.Value) { /* ... (jak poprzednio) ... */ }
						// Nie można użyć DisplayElement bezpośrednio z kolorowaniem symbolu
						// Uproszczenie: wyświetlamy tylko kartę lub puste miejsce
					}
				}
				DisplayElement(ActiveArea.Foundation, i, 0, display, "");
				Console.Write(" ");
			}
			Console.WriteLine($"\nLiczba ruchów: {game.MovesCount}");
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80));

			// --- Kolumny Tableau [T1-T7] ---
			Console.WriteLine("Kolumny Gry [T1-T7]:");
			int maxRows = game.Tableaux.Max(t => t.Count);

			for (int row = 0; row < maxRows; row++) {
				for (int col = 0; col < 7; col++) {
					var currentTableau = game.Tableaux[col];
					if (row < currentTableau.Count) {
						Card card = currentTableau.GetCardsForDisplay()[row];
						DisplayElement(ActiveArea.Tableau, col, row, card.GetDisplay(), "", card.IsFaceUp); // Tylko odkryte są "targetable"
						Console.Write(" ");
					} else {
						// Jeśli kursor jest na pustym miejscu w kolumnie (można tam położyć Króla)
						if (currentArea == ActiveArea.Tableau && currentPileIndex == col && currentCardIndex == row && currentTableau.IsEmpty) {
							SetHighlight(true); // Podświetl puste miejsce
							Console.Write("[ K ]"); // Wskaż, że można tu położyć Króla
							SetHighlight(false);
						} else {
							Console.Write("    "); // Puste miejsce
						}

					}
					Console.Write(" "); // Odstęp między kolumnami
				}
				Console.WriteLine();
			}
			Console.WriteLine(new string('-', Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 80));

			// Wyświetl informacje o sterowaniu i komunikat
			Console.WriteLine("Sterowanie: Strzałki - ruch | Enter/Spacja - wybierz/przenieś | Esc - anuluj | D - dobierz | U - cofnij | H - ranking | R - restart | Q - wyjdź");
			if (!string.IsNullOrEmpty(message)) {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine($"Komunikat: {message}");
				Console.ResetColor();
			}
			// Wyświetl informację o zaznaczeniu
			if (selectedPileIndex != -1) {
				Console.ForegroundColor = ConsoleColor.Cyan;
				string sourceDesc = GetPileDescription(selectedArea, selectedPileIndex);
				if (selectedArea == ActiveArea.Tableau && selectedCardIndex != -1) {
					sourceDesc += $" (karta #{selectedCardIndex + 1}{(selectedCardCount > 1 ? $" - {selectedCardCount} kart" : "")})";
				}
				Console.WriteLine($"Zaznaczono źródło: {sourceDesc}. Wybierz cel (Enter/Spacja) lub anuluj (Esc).");
				Console.ResetColor();
			}
		}

		// Pomocnicza metoda do wyświetlania elementu z opcjonalnym podświetleniem
		static void DisplayElement(ActiveArea area, int pileIndex, int cardIndex, string display, string suffix, bool isTargetable = true) {
			bool isCursorOn = isTargetable && currentArea == area && currentPileIndex == pileIndex && currentCardIndex == cardIndex;
			bool isSelected = isTargetable && selectedArea == area && selectedPileIndex == pileIndex &&
							  (area != ActiveArea.Tableau || selectedCardIndex == -1 || (cardIndex >= selectedCardIndex && cardIndex < selectedCardIndex + selectedCardCount));


			// Ustalanie koloru karty (jeśli to karta)
			ConsoleColor cardColor = ConsoleColor.White; // Domyślny dla [ ] lub [ * ]
			if (display.Length == 3 && display[0] != '[' && display[0] != ' ') // Zakładamy format " XY" lub "XYY"
			{
				char suitChar = display[display.Length - 1];
				if (suitChar == '♥' || suitChar == '♦') cardColor = ConsoleColor.Red;
				else if (suitChar == '♣' || suitChar == '♠') cardColor = ConsoleColor.DarkGray; // Ciemnoszary dla czarnych
			}


			if (isSelected) Console.BackgroundColor = ConsoleColor.DarkCyan;
			else if (isCursorOn) Console.BackgroundColor = ConsoleColor.Gray;

			// Ustawienie koloru tekstu karty
			Console.ForegroundColor = (Console.BackgroundColor == ConsoleColor.Black || Console.BackgroundColor == ConsoleColor.DarkBlue) // Domyślne tła
									   ? cardColor
									   : ConsoleColor.Black; // Czarny tekst na jasnym tle


			Console.Write(display);
			Console.ResetColor(); // Resetuj kolory po wyświetleniu elementu
			Console.Write(suffix); // Wyświetl ewentualny sufiks (np. licznik, spacja)
		}

		// Pomocnicza metoda do ustawiania podświetlenia (używana rzadko)
		static void SetHighlight(bool active) {
			if (active) { Console.BackgroundColor = ConsoleColor.Gray; Console.ForegroundColor = ConsoleColor.Black; } else Console.ResetColor();
		}


		// Logika przesuwania kursora
		static void MoveCursor(Game game, int dx, int dy) {
			// Anuluj zaznaczenie przy ruchu kursora
			if (selectedPileIndex != -1) {
				ResetSelection();
				message = "Anulowano zaznaczenie.";
				// Nie ruszaj kursora od razu po anulowaniu, pozwól użytkownikowi zobaczyć efekt
				return;
			}


			ActiveArea targetArea = currentArea;
			int targetPileIndex = currentPileIndex;
			int targetCardIndex = currentCardIndex;

			switch (currentArea) {
				case ActiveArea.Stock:
					if (dx > 0) targetArea = ActiveArea.Waste; // Prawo -> Waste
					else if (dy > 0) { targetArea = ActiveArea.Tableau; targetPileIndex = 0; targetCardIndex = 0; } // Dół -> T1
					break;
				case ActiveArea.Waste:
					if (dx < 0) targetArea = ActiveArea.Stock; // Lewo -> Stock
					else if (dx > 0) { targetArea = ActiveArea.Foundation; targetPileIndex = 0; } // Prawo -> F1
					else if (dy > 0) { targetArea = ActiveArea.Tableau; targetPileIndex = Math.Min(2, game.Tableaux.Count - 1); targetCardIndex = 0; } // Dół -> T2/T3
																																					   // Nawigacja wewnątrz Waste (jeśli są 3 karty) - niezaimplementowane, zawsze celuje w ostatnią
					break;
				case ActiveArea.Foundation:
					if (dx < 0 && currentPileIndex == 0) targetArea = ActiveArea.Waste; // Lewo z F1 -> Waste
					else if (dx < 0) targetPileIndex--; // Lewo w F
					else if (dx > 0 && currentPileIndex < 3) targetPileIndex++; // Prawo w F
					else if (dy > 0) { targetArea = ActiveArea.Tableau; targetPileIndex = Math.Min(currentPileIndex + 3, game.Tableaux.Count - 1); targetCardIndex = 0; } // Dół -> T4-T7
					break;
				case ActiveArea.Tableau:
					if (dy < 0 && currentCardIndex == 0) // Góra z wierzchu Tableau
					{
						if (currentPileIndex == 0) targetArea = ActiveArea.Stock;
						else if (currentPileIndex <= 2) targetArea = ActiveArea.Waste;
						else targetArea = ActiveArea.Foundation; targetPileIndex = Math.Min(currentPileIndex - 3, 3);
						targetCardIndex = 0; // Reset card index when moving up
					} else if (dy < 0) targetCardIndex--; // Góra wewnątrz Tableau
					else if (dy > 0) targetCardIndex++; // Dół wewnątrz Tableau
					else if (dx < 0 && currentPileIndex > 0) targetPileIndex--; // Lewo w T
					else if (dx > 0 && currentPileIndex < 6) targetPileIndex++; // Prawo w T
					break;
			}

			// Walidacja i aktualizacja pozycji kursora
			ValidateAndSetCursor(game, targetArea, targetPileIndex, targetCardIndex);
		}

		// Sprawdza poprawność nowej pozycji kursora i ją ustawia
		static void ValidateAndSetCursor(Game game, ActiveArea area, int pileIndex, int cardIndex) {
			int maxPileIndex = 0;
			int maxCardIndex = 0;

			switch (area) {
				case ActiveArea.Stock: pileIndex = 0; cardIndex = 0; break;
				case ActiveArea.Waste:
					pileIndex = 0;
					// Zawsze celuj w ostatnią kartę w Waste (lub puste miejsce)
					cardIndex = 0; // Uproszczenie: zawsze indeks 0 dla Waste w logice kursora
					break;
				case ActiveArea.Foundation:
					maxPileIndex = 3;
					pileIndex = Math.Clamp(pileIndex, 0, maxPileIndex);
					cardIndex = 0; // W Foundation zawsze celujemy w cały stos
					break;
				case ActiveArea.Tableau:
					maxPileIndex = 6;
					pileIndex = Math.Clamp(pileIndex, 0, maxPileIndex);
					// Ustal maksymalny indeks karty (liczba kart lub 0 jeśli pusta)
					maxCardIndex = Math.Max(0, game.Tableaux[pileIndex].Count - 1);
					// Jeśli kolumna jest pusta, dozwolony indeks to 0 (reprezentuje puste miejsce)
					if (game.Tableaux[pileIndex].IsEmpty) maxCardIndex = 0;
					cardIndex = Math.Clamp(cardIndex, 0, maxCardIndex);

					// Jeśli celujemy w zakrytą kartę, przesuń kursor na ostatnią odkrytą (lub pierwszą, jeśli nie ma odkrytych)
					Card? targetCard = game.Tableaux[pileIndex].GetCardAt(cardIndex);
					if (targetCard != null && !targetCard.IsFaceUp) {
						int firstFaceUp = game.Tableaux[pileIndex].FindFirstFaceUpCardIndex();
						if (firstFaceUp != -1) cardIndex = firstFaceUp; // Przesuń na pierwszą odkrytą
						else if (game.Tableaux[pileIndex].Count > 0) cardIndex = game.Tableaux[pileIndex].Count - 1; // Przesuń na ostatnią zakrytą, jeśli nie ma odkrytych
						else cardIndex = 0; // Pusta kolumna
					}
					break;
			}

			currentArea = area;
			currentPileIndex = pileIndex;
			currentCardIndex = cardIndex;
		}

		// Obsługa Enter/Spacji - zaznaczanie źródła lub wykonywanie ruchu
		static void HandleSelectionOrMove(Game game) {
			PileType sourceType = PileType.Stock;
			int sourceIndex = -1;
			int sourceCardIndex = -1; // Tylko dla Tableau
			int sourceCardCount = 1;

			PileType destType = PileType.Stock;
			int destIndex = -1;

			// --- Jeśli nic nie jest zaznaczone -> zaznacz źródło ---
			if (selectedPileIndex == -1) {
				// Sprawdź, czy można zaznaczyć element pod kursorem
				switch (currentArea) {
					case ActiveArea.Stock:
						message = "Nie można przenieść kart bezpośrednio ze stosu [S]. Użyj 'D', aby dobrać.";
						return; // Nie można wybrać Stock jako źródła
					case ActiveArea.Waste:
						if (game.Waste.IsEmpty) { message = "Stos [W] jest pusty."; return; }
						selectedArea = ActiveArea.Waste;
						selectedPileIndex = 0;
						selectedCardIndex = -1; // Nie dotyczy
						selectedCardCount = 1;
						message = $"Zaznaczono źródło: {GetPileDescription(selectedArea, selectedPileIndex)}. Wybierz cel.";
						break;
					case ActiveArea.Foundation:
						if (game.Foundations[currentPileIndex].IsEmpty) { message = "Stos [F] jest pusty."; return; }
						selectedArea = ActiveArea.Foundation;
						selectedPileIndex = currentPileIndex;
						selectedCardIndex = -1; // Nie dotyczy
						selectedCardCount = 1;
						message = $"Zaznaczono źródło: {GetPileDescription(selectedArea, selectedPileIndex)}. Wybierz cel.";
						break;
					case ActiveArea.Tableau:
						var tableau = game.Tableaux[currentPileIndex];
						if (tableau.IsEmpty) { message = "Kolumna [T] jest pusta."; return; }
						// Sprawdź, czy karta pod kursorem jest odkryta
						Card? card = tableau.GetCardAt(currentCardIndex);
						if (card == null || !card.IsFaceUp) { message = "Nie można zaznaczyć zakrytej karty."; return; }

						selectedArea = ActiveArea.Tableau;
						selectedPileIndex = currentPileIndex;
						selectedCardIndex = currentCardIndex; // Zapamiętaj, którą kartę kliknięto
						selectedCardCount = tableau.Count - currentCardIndex; // Zaznacz od tej karty do końca
						message = $"Zaznaczono źródło: {GetPileDescription(selectedArea, selectedPileIndex)} ({selectedCardCount} kart). Wybierz cel.";
						break;
				}
			}
			// --- Jeśli źródło jest zaznaczone -> wykonaj ruch ---
			else {
				// Pobierz typ i indeks źródła
				sourceType = ConvertAreaToPileType(selectedArea);
				sourceIndex = selectedPileIndex;
				sourceCardCount = selectedCardCount; // Użyj zapisanej liczby kart

				// Pobierz typ i indeks celu (z aktualnej pozycji kursora)
				destType = ConvertAreaToPileType(currentArea);
				destIndex = currentPileIndex;

				// Sprawdzenia poprawności celu
				if (destType == PileType.Stock || destType == PileType.Waste) {
					message = "Nieprawidłowy cel ruchu.";
					ResetSelection(); // Anuluj zaznaczenie po błędzie
					return;
				}
				// Nie można przenieść na ten sam stos
				if (sourceType == destType && sourceIndex == destIndex) {
					message = "Nie można przenieść na ten sam stos.";
					ResetSelection();
					return;
				}
				// Wykonaj próbę ruchu
				if (game.TryMove(sourceType, sourceIndex, destType, destIndex, sourceCardCount)) {
					message = $"Przeniesiono z {GetPileDescription(selectedArea, selectedPileIndex)} do {GetPileDescription(currentArea, currentPileIndex)}.";
				} else {
					message = "Ruch niedozwolony.";
				}
				// Zawsze resetuj zaznaczenie po próbie ruchu (udanej lub nie)
				ResetSelection();
			}
		}

		// Resetuje stan zaznaczenia
		static void ResetSelection() {
			selectedPileIndex = -1;
			selectedCardIndex = -1;
			selectedCardCount = 0;
		}

		// Konwertuje ActiveArea na PileType
		static PileType ConvertAreaToPileType(ActiveArea area) {
			switch (area) {
				case ActiveArea.Stock: return PileType.Stock;
				case ActiveArea.Waste: return PileType.Waste;
				case ActiveArea.Foundation: return PileType.Foundation;
				case ActiveArea.Tableau: return PileType.Tableau;
				default: throw new ArgumentOutOfRangeException(nameof(area));
			}
		}

		// Zwraca opis stosu dla komunikatów
		static string GetPileDescription(ActiveArea area, int index) {
			switch (area) {
				case ActiveArea.Stock: return "Stos [S]";
				case ActiveArea.Waste: return "Odrzucone [W]";
				case ActiveArea.Foundation: return $"Fundacja [F{index + 1}]";
				case ActiveArea.Tableau: return $"Kolumna [T{index + 1}]";
				default: return "Nieznany";
			}
		}


		// Metoda do wyboru poziomu trudności - bez zmian
		static bool? ChooseDifficulty() {
			Console.Clear(); Console.WriteLine("Wybierz poziom trudności:\n 1. Łatwy (dobieranie 1 karty)\n 2. Trudny (dobieranie 3 kart)\n 3. Wyjdź"); Console.Write("Wybór: ");
			while (true) {
				string? choice = Console.ReadLine();
				switch (choice) {
					case "1": Console.WriteLine("Wybrano poziom łatwy."); System.Threading.Thread.Sleep(500); return false;
					case "2": Console.WriteLine("Wybrano poziom trudny."); System.Threading.Thread.Sleep(500); return true;
					case "3": return null;
					default: Console.Write("Nieprawidłowy wybór. Wpisz 1, 2 lub 3: "); break;
				}
			}
		}
	}
}
