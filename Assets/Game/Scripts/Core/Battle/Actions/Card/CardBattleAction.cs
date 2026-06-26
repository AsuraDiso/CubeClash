using System;
using Game.Scripts.Core.Battle.Actions;

namespace Game.Scripts.Core.Battle.Actions.Card
{
    public sealed class CardBattleAction : IBattleAction
    {
        public string TypeId => BattleActionTypes.CardResolve;
        public int DeckCardIndex { get; }
        public int[] DieIndices { get; }

        public CardBattleAction(int deckCardIndex, int[] dieIndices)
        {
            DeckCardIndex = deckCardIndex;
            DieIndices = dieIndices ?? Array.Empty<int>();
        }

        public static bool TryDecode(int[] payload, out IBattleAction action)
        {
            action = null;
            if (payload == null || payload.Length < 1)
                return false;

            var dieIndices = new int[payload.Length - 1];
            if (dieIndices.Length > 0)
                Array.Copy(payload, 1, dieIndices, 0, dieIndices.Length);

            action = new CardBattleAction(payload[0], dieIndices);
            return true;
        }
    }
}
