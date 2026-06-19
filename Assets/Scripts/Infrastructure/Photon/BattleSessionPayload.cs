using System.Collections.Generic;
using Cards;
using Core.Data;

namespace Infrastructure.Photon
{
    public sealed class BattleSessionPayload
    {
        public BattleSessionPayload(
            PlayerProfile localProfile,
            IReadOnlyList<PlacedCard> localDeck,
            CardCatalog cardCatalog)
        {
            LocalProfile = localProfile;
            LocalDeck = localDeck;
            CardCatalog = cardCatalog;
        }

        public PlayerProfile LocalProfile { get; }
        public IReadOnlyList<PlacedCard> LocalDeck { get; }
        public CardCatalog CardCatalog { get; }
    }
}
