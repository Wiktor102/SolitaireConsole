namespace SolitaireConsole {
    // Stos rezerwowy (Stock) - skąd dobieramy karty
    public class StockPile : CardPile {
        private Deck deck; // Talia używana do gry

        // Konstruktor inicjalizujący stos rezerwowy z nowej talii
        public StockPile() {
            deck = new Deck(); // Tworzy nową, potasowaną talię
            // Przenosi wszystkie karty z talii do stosu rezerwowego
            while (deck.Count > 0) {
                Card? card = deck.Deal();
                if (card != null) {
                    card.IsFaceUp = false; // Karty w stosie rezerwowym są zakryte
                    AddCard(card);
                }
            }
        }

        // Metoda do rozdania kart na początku gry do kolumn Tableau
        public List<Card> DealInitialTableauCards(int count) {
            return RemoveTopCards(count);
        }

        // Metoda do dobrania kart ze stosu rezerwowego
        // Zwraca listę dobranych kart (1 lub 3 w zależności od trudności)
        public List<Card> Draw(int drawCount) {
            int actualDrawCount = Math.Min(drawCount, cards.Count); // Nie można dobrać więcej niż jest
            List<Card> drawnCards = RemoveTopCards(actualDrawCount);
            // Odkrywamy dobrane karty
            foreach (var card in drawnCards) {
                card.IsFaceUp = true;
            }
            return drawnCards;
        }

        // Metoda do resetowania stosu rezerwowego (przeniesienie kart z Waste z powrotem)
        public void Reset(IEnumerable<Card> wasteCards) {
            // Dodajemy karty z Waste z powrotem do Stock
            AddCards(wasteCards.Reverse()); // Odwracamy kolejność, aby zachować porządek dobierania
            // Tasujemy ponownie (zgodnie ze specyfikacją)
            ShuffleStock();
        }

        // Prywatna metoda do tasowania kart w stosie rezerwowym
        private void ShuffleStock() {
            int n = cards.Count;
            var rng = new Random();
            while (n > 1) {
                n--;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
            // Upewniamy się, że wszystkie karty są zakryte po przetasowaniu
            foreach (var card in cards) {
                card.IsFaceUp = false;
            }
        }

        // Stos rezerwowy nie przyjmuje kart bezpośrednio (tylko przez Reset)
        public override bool CanAddCard(Card card) {
            return false;
        }
    }

    // Stos kart odrzuconych (Waste) - gdzie trafiają karty ze Stock
    public class WastePile : CardPile {
        // Konstruktor
        public WastePile() : base() { }

        // Metoda do dodawania kart dobranych ze Stock
        public override void AddCards(IEnumerable<Card> cardsToAdd) {
            foreach (var card in cardsToAdd) {
                card.IsFaceUp = true; // Karty w Waste są zawsze odkryte
            }
            base.AddCards(cardsToAdd); // Dodaje karty na wierzch
        }

        // Zwraca kartę, którą można zagrać (zawsze wierzchnia)
        public Card? GetPlayableCard() {
            return PeekTopCard();
        }

        // Usuwa wierzchnią kartę (po zagraniu jej)
        public override Card? RemoveTopCard() {
            return base.RemoveTopCard();
        }

        // Waste Pile nie przyjmuje kart w standardowy sposób (tylko ze Stock)
        public override bool CanAddCard(Card card) {
            return false;
        }
    }

    // Stos końcowy (Foundation) - gdzie układamy karty od Asa do Króla
    public class FoundationPile : CardPile {
        public Suit PileSuit { get; private set; } // Kolor (Suit) tego stosu
        private bool suitSet = false; // Czy kolor stosu został już ustalony?

        // Konstruktor
        public FoundationPile() : base() { }

        // Sprawdza, czy można dodać kartę na ten stos
        public override bool CanAddCard(Card card) {
            // Jeśli stos jest pusty, można dodać tylko Asa
            if (IsEmpty) {
                return card.Rank == Rank.Ace;
            } else {
                // Jeśli stos nie jest pusty, sprawdzamy wierzchnią kartę
                Card? topCard = PeekTopCard();
                if (topCard != null) {
                    // Karta musi być tego samego koloru (Suit)
                    // i o jeden stopień wyższa (Rank) niż wierzchnia karta
                    return card.Suit == topCard.Suit && card.Rank == topCard.Rank + 1;
                }
                return false; // Nie powinno się zdarzyć, ale dla bezpieczeństwa
            }
        }

        // Dodaje kartę na stos (nadpisuje bazową metodę)
        public override void AddCard(Card card) {
            if (CanAddCard(card)) {
                // Jeśli to pierwsza karta (As), ustawiamy kolor (Suit) stosu
                if (IsEmpty) {
                    PileSuit = card.Suit;
                    suitSet = true;
                }
                card.IsFaceUp = true; // Karty na Foundation są zawsze odkryte
                base.AddCard(card);
            } else {
                // Rzucenie wyjątku lub obsługa błędu, jeśli próba dodania nieprawidłowej karty
                // Console.WriteLine("Błąd: Nie można dodać tej karty na stos końcowy.");
            }
        }

        // Metoda do resetowania koloru stosu (np. przy cofaniu ruchu Asa)
        public void ResetSuitIfEmpty() {
            if (IsEmpty) {
                suitSet = false;
                // PileSuit technicznie pozostaje, ale suitSet decyduje
            }
        }

        // Zwraca kolor (Suit) przypisany do tego stosu, jeśli został ustawiony
        public Suit? GetPileSuit() {
            return suitSet ? PileSuit : (Suit?)null;
        }
    }

    // Kolumna gry (Tableau) - 7 kolumn na planszy
    public class TableauPile : CardPile {
        // Konstruktor
        public TableauPile() : base() { }

        // Metoda do inicjalnego rozdania kart do kolumny
        public void DealInitialCards(List<Card> initialCards) {
            cards.AddRange(initialCards);
            // Odkrywamy tylko ostatnią kartę
            if (cards.Count > 0) {
                cards.Last().IsFaceUp = true;
            }
        }

        // Sprawdza, czy można dodać pojedynczą kartę na wierzch tej kolumny
        public override bool CanAddCard(Card card) {
            // Jeśli kolumna jest pusta, można dodać tylko Króla (K)
            if (IsEmpty) {
                return card.Rank == Rank.King;
            } else {
                // Jeśli kolumna nie jest pusta, sprawdzamy wierzchnią kartę
                Card? topCard = PeekTopCard();
                if (topCard != null && topCard.IsFaceUp) // Musi być odkryta
                {
                    // Karta musi być przeciwnego koloru (Red/Black)
                    // i o jeden stopień niższa (Rank) niż wierzchnia karta
                    return card.Color != topCard.Color && card.Rank == topCard.Rank - 1;
                }
                return false; // Nie można dodać na zakrytą kartę
            }
        }

        // Sprawdza, czy można dodać sekwencję kart na wierzch tej kolumny
        // (Pierwsza karta sekwencji musi pasować do wierzchniej karty kolumny)
        public bool CanAddSequence(List<Card> sequence) {
            if (sequence == null || sequence.Count == 0 || !sequence.First().IsFaceUp) {
                return false; // Sekwencja musi istnieć i pierwsza karta musi być odkryta
            }
            // Sprawdza, czy pierwsza karta sekwencji pasuje do wierzchu kolumny
            return CanAddCard(sequence.First());
        }

        // Odkrywa wierzchnią kartę, jeśli jest zakryta
        public bool FlipTopCardIfNecessary() {
            if (!IsEmpty && !cards.Last().IsFaceUp) {
                cards.Last().IsFaceUp = true;
                return true; // Karta została odkryta
            }
            return false; // Karta już była odkryta lub stos jest pusty
        }

        // Znajduje indeks pierwszej odkrytej karty od góry
        // Zwraca -1, jeśli nie ma odkrytych kart
        public int FindFirstFaceUpCardIndex() {
            for (int i = 0; i < cards.Count; i++) {
                if (cards[i].IsFaceUp) {
                    return i;
                }
            }
            return -1;
        }

        // Pobiera sekwencję odkrytych kart, zaczynając od podanego indeksu
        public List<Card> GetFaceUpSequence(int startIndex) {
            if (startIndex < 0 || startIndex >= cards.Count || !cards[startIndex].IsFaceUp) {
                return new List<Card>(); // Zwraca pustą listę, jeśli indeks jest nieprawidłowy lub karta jest zakryta
            }
            // Zwraca wszystkie karty od startIndex do końca
            return cards.GetRange(startIndex, cards.Count - startIndex);
        }

        // Usuwa sekwencję kart zaczynającą się od podanego indeksu
        public List<Card> RemoveSequence(int startIndex) {
            if (startIndex < 0 || startIndex >= cards.Count) {
                return new List<Card>();
            }
            int count = cards.Count - startIndex;
            List<Card> removed = cards.GetRange(startIndex, count);
            cards.RemoveRange(startIndex, count);
            return removed;
        }

        // Zwraca listę kart w kolumnie (do wyświetlania)
        public List<Card> GetCardsForDisplay() {
            return cards;
        }
    }
}