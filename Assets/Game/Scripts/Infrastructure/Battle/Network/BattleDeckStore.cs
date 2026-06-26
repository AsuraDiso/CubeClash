using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Infrastructure.Battle.Network
{
    internal sealed class BattleDeckStore
    {
        private List<PlacedCard> _localDeck = new();
        private List<PlacedCard> _opponentDeck = new();

        public IReadOnlyList<PlacedCard> LocalDeck => _localDeck;
        public IReadOnlyList<PlacedCard> OpponentDeck => _opponentDeck;

        public void SetLocalPreview(IReadOnlyList<PlacedCard> deck) =>
            _localDeck = new List<PlacedCard>(deck);

        public void ApplyDeck(int networkPlayerId, int localNetworkPlayerId, IReadOnlyList<PlacedCard> deck)
        {
            if (networkPlayerId == localNetworkPlayerId)
                _localDeck = new List<PlacedCard>(deck);
            else
                _opponentDeck = new List<PlacedCard>(deck);
        }
    }
}
