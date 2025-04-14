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
}