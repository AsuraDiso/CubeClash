using System.Collections.Generic;
using Game.Scripts.Core.Data.Cards;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Data;

namespace Game.Scripts.Infrastructure.Battle.Session
{
    public sealed class BattleSessionPayload
    {
        public BattleSessionPayload(PlayerProfile localProfile, IReadOnlyList<PlacedCard> localDeck, CardCatalog cardCatalog,
            BattleModeConfig battleModeConfig)
        {
            LocalProfile = localProfile;
            LocalDeck = localDeck;
            CardCatalog = cardCatalog;
            BattleModeConfig = battleModeConfig;
        }

        public PlayerProfile LocalProfile { get; }
        public IReadOnlyList<PlacedCard> LocalDeck { get; }
        public CardCatalog CardCatalog { get; }
        public BattleModeConfig BattleModeConfig { get; }
    }
}
