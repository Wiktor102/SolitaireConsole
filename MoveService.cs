using SolitaireConsole.CardPiles;
using SolitaireConsole.Utils;

namespace SolitaireConsole
{
    internal class MoveService
    {
        private readonly Game _game;

        public MoveService(Game game)
        {
            _game = game;
        }

        public bool TryMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, int cardCount = 1)
        {
            // 1. Retrieve piles
            var sourcePile = GetPile(sourceType, sourceIndex);
            var destPile = GetPile(destType, destIndex);
            ValidatePiles(sourceType, sourceIndex, destType, destIndex, sourcePile, destPile); // Throws if invalid

            // 2. Determine cards to move
            var movedCards = ExtractMovedCards(sourceType, sourceIndex, cardCount, sourcePile!);
            if (movedCards.Count == 0)
            {
                throw new EmptyPileException("Brak kart do przeniesienia.");
            }

            // 3. Validate destination rules
            try
            {
                ValidateDestination(destType, destPile!, movedCards);
            }
            catch (MoveException)
            {
                sourcePile!.AddCards(movedCards); // Restore cards before re-throwing
                throw;
            }

            // 4. Handle source flip and foundation suit
            bool wasFlipped = HandleSourceFlip(sourceType, sourcePile!);
            bool wasFoundationSet = HandleFoundationSetup(destType, destPile!);

            // 5. Execute move
            PlaceCards(destPile!, movedCards);

            // 6. Record move and increment count
            RecordMove(sourceType, sourceIndex, destType, destIndex, movedCards, wasFlipped, wasFoundationSet);
            return true;
        }

        // Validate that both source and destination piles exist
        private void ValidatePiles(PileType sourceType, int sourceIndex, PileType destType, int destIndex, CardPile? sourcePile, CardPile? destPile)
        {
            if (sourcePile == null)
            {
                throw new InvalidPileException($"Nieprawidłowy stos źródłowy: {sourceType}{(sourceIndex >= 0 ? sourceIndex + 1 : "")}");
            }
            if (destPile == null)
            {
                throw new InvalidPileException($"Nieprawidłowy stos docelowy: {destType}{(destIndex >= 0 ? destIndex + 1 : "")}");
            }
        }

        // Extract the cards to move (single card or sequence)
        private List<Card> ExtractMovedCards(PileType sourceType, int sourceIndex, int cardCount, CardPile sourcePile)
        {
            if (sourcePile.IsEmpty)
            {
                throw new EmptyPileException("Stos źródłowy jest pusty.");
            }
            if (sourceType == PileType.Tableau && cardCount > 1 && sourcePile is TableauPile tableau)
            {
                var sequence = tableau.GetFaceUpSequence(sourceIndex); // Check based on original sourceIndex for tableau sequence
                if (sequence.Count < cardCount) {
                    throw new InvalidCardSequenceException("Niewystarczająca liczba odkrytych kart w sekwencji.");
                }
                // Validate the sequence itself before attempting to remove
                for (int i = 0; i < cardCount -1; i++) {
                    if (sequence[i].Color == sequence[i+1].Color || sequence[i].Rank != sequence[i+1].Rank + 1) {
                        throw new InvalidCardSequenceException("Niepoprawna sekwencja kart do przeniesienia (kolor lub wartość).");
                    }
                }
                return tableau.RemoveSequence(tableau.Cards.Count - cardCount); // Remove from end of pile
            }
            if (cardCount > sourcePile.Count)
            {
                throw new InvalidCardSequenceException("Nie można przenieść więcej kart niż jest na stosie.");
            }
            return sourcePile.RemoveTopCards(cardCount);
        }

        // Validate destination rules for single or multiple cards
        private void ValidateDestination(PileType destType, CardPile destPile, List<Card> movedCards)
        {
            if (movedCards.Count == 1)
            {
                if (!destPile.CanAddCard(movedCards[0]))
                {
                    throw new InvalidMoveRuleException("Nie można przenieść karty na wybrany stos (reguły).");
                }
            }
            else if (destType == PileType.Tableau && destPile is TableauPile tableau)
            {
                if (!tableau.CanAddSequence(movedCards))
                {
                    throw new InvalidMoveRuleException("Nie można przenieść sekwencji kart na wybrany stos (reguły).");
                }
            }
            else if (movedCards.Count > 1 && destType != PileType.Tableau)
            {
                throw new InvalidMoveRuleException("Sekwencje kart można przenosić tylko na stosy Tableau.");
            }
        }

        // Flip the top card of source if needed
        private bool HandleSourceFlip(PileType sourceType, CardPile sourcePile)
        {
            if (sourceType == PileType.Tableau && sourcePile is TableauPile tableau)
            {
                return tableau.FlipTopCardIfNecessary();
            }
            return false;
        }

        // Determine if foundation suit was set on an empty foundation
        private bool HandleFoundationSetup(PileType destType, CardPile destPile)
        {
            return destType == PileType.Foundation && destPile.Count == 0;
        }

        // Place moved cards onto destination pile
        private void PlaceCards(CardPile destPile, List<Card> movedCards)
        {
            if (movedCards.Count == 1) destPile.AddCard(movedCards[0]);
            else destPile.AddCards(movedCards);
        }

        // Record the move and increment counters
        private void RecordMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, List<Card> movedCards, bool wasFlipped, bool wasFoundationSet)
        {
            var record = new MoveRecord(sourceType, sourceIndex, destType, destIndex, movedCards, wasFlipped, wasFoundationSet);
            _game.AddMoveRecord(record);
            _game.IncrementMoveCount();
        }

        private CardPile? GetPile(PileType type, int index)
        {
            return type switch
            {
                PileType.Stock => _game.Stock,
                PileType.Waste => _game.Waste,
                PileType.Foundation => _game.Foundations.ElementAtOrDefault(index),
                PileType.Tableau => _game.Tableaux.ElementAtOrDefault(index),
                _ => null,
            };
        }
    }
}