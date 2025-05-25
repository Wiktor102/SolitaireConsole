using SolitaireConsole.CardPiles;
using SolitaireConsole.Utils;
using SolitaireConsole.UI;

namespace SolitaireConsole {
	/// <summary>
	/// Serwis odpowiedzialny za obsługę ruchów w grze.
	/// </summary>
	internal class MoveService {
		private readonly Game _game;
		private readonly GameSettings _gameSettings;

		/// <summary>
		/// Tworzy nowy MoveService dla danej gry i ustawień.
		/// </summary>
		/// <param name="game">Obiekt gry.</param>
		/// <param name="gameSettings">Ustawienia gry.</param>
		public MoveService(Game game, GameSettings gameSettings) {
			_game = game;
			_gameSettings = gameSettings;
		}

		/// <summary>
		/// Próbuje wykonać ruch przeniesienia kart między stosami.
		/// </summary>
		/// <param name="sourceType">Typ stosu źródłowego.</param>
		/// <param name="sourceIndex">Indeks stosu źródłowego.</param>
		/// <param name="destType">Typ stosu docelowego.</param>
		/// <param name="destIndex">Indeks stosu docelowego.</param>
		/// <param name="cardCount">Liczba przenoszonych kart.</param>
		/// <returns>Czy ruch się powiódł.</returns>
		public bool TryMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, int cardCount = 1) {
			// 1. Pobierz stosy
			var sourcePile = GetPile(sourceType, sourceIndex);
			var destPile = GetPile(destType, destIndex);
			ValidatePiles(sourceType, sourceIndex, destType, destIndex, sourcePile, destPile); // Wyrzuca wyjątek, jeśli stosy są nieprawidłowe

			// 2. Określ karty do przeniesienia
			var movedCards = ExtractMovedCards(sourceType, cardCount, sourcePile!);
			if (movedCards.Count == 0) {
				throw new EmptyPileException("Brak kart do przeniesienia.");
			}

			// 3. Walidacja reguł stosu docelowego
			try {
				ValidateDestination(destType, destPile!, movedCards);
			} catch (MoveException) {
				sourcePile!.AddCards(movedCards); // Przywróć karty do stosu źródłowego
				throw;
			}

			// 4. Obsłuż odkrycie karty w stosie źródłowym i ustawienie koloru w fundamencie
			bool wasFlipped = HandleSourceFlip(sourceType, sourcePile!);
			bool wasFoundationSet = HandleFoundationSetup(destType, destPile!);

			// 5. Wykonaj przeniesienie
			PlaceCards(destPile!, movedCards);

			// 6. Zapisz ruch i zwiększ licznik
			RecordMove(sourceType, sourceIndex, destType, destIndex, movedCards, wasFlipped, wasFoundationSet);
			return true;
		}

		/// <summary>
		/// Próbuje automatycznie przenieść kartę na stos końcowy (Foundation), jeśli opcja jest włączona.
		/// </summary>
		/// <param name="sourceType">Typ stosu źródłowego.</param>
		/// <param name="sourceIndex">Indeks stosu źródłowego.</param>
		/// <returns>Czy ruch się powiódł.</returns>
		public bool TryAutoMoveToFoundation(PileType sourceType, int sourceIndex) {
			if (!_gameSettings.AutoMoveToFoundation) return false; // Sprawdź czy automatyczne przenoszenie jest włączone
			return TryManualMoveToFoundation(sourceType, sourceIndex);
		}

		/// <summary>
		/// Ręczne przeniesienie karty na stos końcowy (Foundation), niezależnie od ustawienia automatycznego przenoszenia.
		/// </summary>
		/// <param name="sourceType">Typ stosu źródłowego.</param>
		/// <param name="sourceIndex">Indeks stosu źródłowego.</param>
		/// <returns>Czy ruch się powiódł.</returns>
		public bool TryManualMoveToFoundation(PileType sourceType, int sourceIndex) {
			var sourcePile = GetPile(sourceType, sourceIndex);
			if (sourcePile == null || sourcePile.IsEmpty) return false; // Stos źródłowy nie istnieje lub jest pusty

			Card? cardToConsiderForMove;

			if (sourceType == PileType.Tableau) {
				if (sourcePile is TableauPile tableauPile) {
					cardToConsiderForMove = tableauPile.Cards.LastOrDefault();
					// Sprawdź, czy karta jest odkryta
					if (cardToConsiderForMove == null || !cardToConsiderForMove.IsFaceUp) {
						return false;
					}
				} else {
					return false; // Nie powinno się zdarzyć jeśli GetPile działa poprawnie
				}
			} else if (sourceType == PileType.Waste) {
				cardToConsiderForMove = sourcePile.PeekTopCard();
				if (cardToConsiderForMove == null) {
					return false; // Stos Waste jest pusty
				}
			} else {
				return false; // Przenoszenie do Foundation jest możliwe tylko z Tableau lub Waste
			}

			// FZnajdź odpowiedni stos końcowy (Foundation)
			for (int i = 0; i < _game.Foundations.Count; i++) {
				var foundationPile = _game.Foundations[i];
				if (foundationPile.CanAddCard(cardToConsiderForMove)) {
					// Spróbuj wykonać ruch. TryMove przeprowadzi szczegółową walidację,
					// w tym ponownie sprawdzi IsFaceUp poprzez ExtractMovedCards dla Tableau.
					return TryMove(sourceType, sourceIndex, PileType.Foundation, i, 1);
				}
			}

			return false; // Nie znaleziono odpowiedniego stosu końcowego
		}

		/// <summary>
		/// Waliduje, czy oba stosy (źródłowy i docelowy) istnieją.
		/// </summary>
		private void ValidatePiles(PileType sourceType, int sourceIndex, PileType destType, int destIndex, CardPile? sourcePile, CardPile? destPile) {
			if (sourcePile == null) throw new InvalidPileException($"Nieprawidłowy stos źródłowy: {sourceType}{(sourceIndex >= 0 ? sourceIndex + 1 : "")}");
			if (destPile == null) throw new InvalidPileException($"Nieprawidłowy stos docelowy: {destType}{(destIndex >= 0 ? destIndex + 1 : "")}");
		}

		/// <summary>
		/// Pobiera karty do przeniesienia (pojedyncza karta lub sekwencja).
		/// </summary>
		private List<Card> ExtractMovedCards(PileType sourceType, int cardCount, CardPile sourcePile) {
			if (sourcePile.IsEmpty) throw new EmptyPileException("Stos źródłowy jest pusty.");
			if (sourceType == PileType.Tableau && sourcePile is TableauPile tableau) {
				if (cardCount > 1) {
					int startIndexInPile = tableau.Cards.Count - cardCount;
					var sequence = tableau.GetFaceUpSequence(startIndexInPile);
					if (sequence.Count < cardCount) throw new InvalidCardSequenceException("Niewystarczająca liczba odkrytych kart w sekwencji.");

					// Sprawdź, czy sekwencja jest poprawna (kolor i wartość)
					for (int i = 0; i < cardCount - 1; i++) {
						if (sequence[i].Color == sequence[i + 1].Color || sequence[i].Rank != sequence[i + 1].Rank + 1) {
							throw new InvalidCardSequenceException("Niepoprawna sekwencja kart do przeniesienia (kolor lub wartość).");
						}
					}

					return tableau.RemoveSequence(tableau.Cards.Count - cardCount); // Remove from end of pile
				}

				// Przenoszenie pojedyńczej karty ze stosu Tableau
				Card? topCard = tableau.Cards.LastOrDefault();
				if (topCard == null || !topCard.IsFaceUp) { // Sprawdź czy każda karta jest na pewno odkryta
					throw new InvalidMoveRuleException("Nie można przenieść zakrytej karty lub gdy stos Tableau jest pusty.");
				}
			}

			// Poniższy kod przenosi:
			// - Pojedynczą kartę z Tableau (po sprawdzeniu IsFaceUp)
			// - Karty z Waste lub innych stosów
			if (cardCount > sourcePile.Count) throw new InvalidCardSequenceException("Nie można przenieść więcej kart niż jest na stosie.");
			return sourcePile.RemoveTopCards(cardCount);
		}

		/// <summary>
		/// Waliduje reguły docelowego stosu dla pojedynczej karty lub sekwencji.
		/// </summary>
		private void ValidateDestination(PileType destType, CardPile destPile, List<Card> movedCards) {
			if (movedCards.Count == 1) {
				if (!destPile.CanAddCard(movedCards[0])) {
					throw new InvalidMoveRuleException("Nie można przenieść karty na wybrany stos (reguły).");
				}
			} else if (destType == PileType.Tableau && destPile is TableauPile tableau) {
				if (!tableau.CanAddSequence(movedCards)) {
					throw new InvalidMoveRuleException("Nie można przenieść sekwencji kart na wybrany stos (reguły).");
				}
			} else if (movedCards.Count > 1 && destType != PileType.Tableau) {
				throw new InvalidMoveRuleException("Sekwencje kart można przenosić tylko na stosy Tableau.");
			}
		}

		/// <summary>
		/// Odkrywa wierzchnią kartę stosu źródłowego, jeśli to konieczne.
		/// </summary>
		private bool HandleSourceFlip(PileType sourceType, CardPile sourcePile) {
			if (sourceType == PileType.Tableau && sourcePile is TableauPile tableau) {
				return tableau.FlipTopCardIfNecessary();
			}
			return false;
		}

		/// <summary>
		/// Określa, czy kolor stosu końcowego został ustawiony na pustym stosie.
		/// </summary>
		private bool HandleFoundationSetup(PileType destType, CardPile destPile) {
			return destType == PileType.Foundation && destPile.Count == 0;
		}

		/// <summary>
		/// Umieszcza przenoszone karty na stosie docelowym.
		/// </summary>
		private void PlaceCards(CardPile destPile, List<Card> movedCards) {
			if (movedCards.Count == 1) destPile.AddCard(movedCards[0]);
			else destPile.AddCards(movedCards);
		}

		/// <summary>
		/// Zapisuje ruch i inkrementuje liczniki.
		/// </summary>
		private void RecordMove(PileType sourceType, int sourceIndex, PileType destType, int destIndex, List<Card> movedCards, bool wasFlipped, bool wasFoundationSet) {
			var record = new MoveRecord(sourceType, sourceIndex, destType, destIndex, movedCards, wasFlipped, wasFoundationSet);
			_game.AddMoveRecord(record);
			_game.IncrementMoveCount();
		}

		/// <summary>
		/// Zwraca stos na podstawie typu i indeksu.
		/// </summary>
		private CardPile? GetPile(PileType type, int index) {
			return type switch {
				PileType.Stock => _game.Stock,
				PileType.Waste => _game.Waste,
				PileType.Foundation => _game.Foundations.ElementAtOrDefault(index),
				PileType.Tableau => _game.Tableaux.ElementAtOrDefault(index),
				_ => null,
			};
		}
	}
}