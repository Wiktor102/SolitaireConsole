namespace SolitaireConsole {
    // Enum określający typ stosu (do identyfikacji w ruchach)
    public enum PileType { Stock, Waste, Foundation, Tableau }

    // Klasa bazowa dla różnych stosów kart (abstrakcyjna)
    public abstract class CardPile {
        protected List<Card> cards; // Lista kart na stosie

        // Konstruktor inicjalizujący pusty stos
        protected CardPile() {
            cards = new List<Card>();
        }

        // Zwraca liczbę kart na stosie
        public int Count => cards.Count;

        // Sprawdza, czy stos jest pusty
        public bool IsEmpty => cards.Count == 0;

        // Dodaje kartę na wierzch stosu
        public virtual void AddCard(Card card) {
            cards.Add(card);
        }

        // Dodaje listę kart na wierzch stosu
        public virtual void AddCards(IEnumerable<Card> cardsToAdd) {
            cards.AddRange(cardsToAdd);
        }

        // Pobiera kartę z wierzchu stosu (bez usuwania)
        // Zwraca null, jeśli stos jest pusty
        public Card? PeekTopCard() {
            return cards.LastOrDefault();
        }

        // Usuwa i zwraca kartę z wierzchu stosu
        // Zwraca null, jeśli stos jest pusty
        public virtual Card? RemoveTopCard() {
            if (IsEmpty) return null;
            Card card = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            return card;
        }

        // Usuwa i zwraca określoną liczbę kart z wierzchu stosu
        public virtual List<Card> RemoveTopCards(int count) {
            if (count <= 0 || count > cards.Count) {
                // Zwraca pustą listę, jeśli żądanie jest nieprawidłowe
                return new List<Card>();
            }
            // Pobiera 'count' ostatnich kart
            List<Card> removed = cards.GetRange(cards.Count - count, count);
            // Usuwa te karty z oryginalnego stosu
            cards.RemoveRange(cards.Count - count, count);
            return removed;
        }

        // Zwraca wszystkie karty ze stosu (bez usuwania)
        public List<Card> GetAllCards() {
            return new List<Card>(cards); // Zwraca kopię listy
        }

        // Czyści stos (usuwa wszystkie karty)
        public void Clear() {
            cards.Clear();
        }

        // Abstrakcyjna metoda sprawdzająca, czy można dodać kartę na ten stos
        // Musi być zaimplementowana przez klasy dziedziczące
        public abstract bool CanAddCard(Card card);
    }

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
}