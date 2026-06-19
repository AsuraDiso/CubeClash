using System.Collections.Generic;
using Cards;
using Core.Battle;

namespace Core.Data
{
    public sealed class DeckBattleLoadoutProvider : IBattleLoadoutProvider
    {
        private readonly IDeckService _deckService;

        public DeckBattleLoadoutProvider(IDeckService deckService)
        {
            _deckService = deckService;
        }

        public IReadOnlyList<PlacedCard> SelectedDeck =>
            _deckService.GetDeck(_deckService.SelectedDeckIndex);
    }
}
