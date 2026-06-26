using System.Collections.Generic;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;
using Game.Scripts.Core.Battle.Actions.Card;

namespace Game.Scripts.Core.Battle.Simulation
{
    public static class BattleActionHandlers
    {
        public static IEnumerable<IBattleActionHandler> CreateDefault(BattleModeConfig mode) =>
            new IBattleActionHandler[]
            {
                new CardBattleActionHandler(mode),
            };
    }
}
