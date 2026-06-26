using System;
using Game.Scripts.Core.Battle.Actions.Card;

namespace Game.Scripts.Core.Battle.Actions
{
    public static class BattleActionCodec
    {
        public static bool TryEncode(IBattleAction action, out string typeId, out int[] payload)
        {
            typeId = action.TypeId;
            payload = null;

            if (action is CardBattleAction card)
            {
                payload = new int[1 + card.DieIndices.Length];
                payload[0] = card.DeckCardIndex;
                Array.Copy(card.DieIndices, 0, payload, 1, card.DieIndices.Length);
                return true;
            }

            return false;
        }

        public static bool TryDecode(string typeId, int[] payload, out IBattleAction action)
        {
            action = null;

            if (typeId == BattleActionTypes.CardResolve)
                return CardBattleAction.TryDecode(payload, out action);

            return false;
        }

        public static bool TryEncodeResult(BattleActionResult result, out int[] payload)
        {
            payload = null;

            if (result is CardBattleActionResult card)
            {
                var dieCount = card.ConsumedDieIndices.Length;
                payload = new int[2 + dieCount];
                payload[0] = card.DeckCardIndex;
                payload[1] = card.Damage;
                Array.Copy(card.ConsumedDieIndices, 0, payload, 2, dieCount);
                return true;
            }

            return false;
        }

        public static bool TryDecodeCardResult(int[] payload, out int deckCardIndex, out int damage,
            out int[] consumedDieIndices)
        {
            deckCardIndex = 0;
            damage = 0;
            consumedDieIndices = Array.Empty<int>();

            if (payload == null || payload.Length < 2)
                return false;

            deckCardIndex = payload[0];
            damage = payload[1];
            if (payload.Length > 2)
            {
                consumedDieIndices = new int[payload.Length - 2];
                Array.Copy(payload, 2, consumedDieIndices, 0, consumedDieIndices.Length);
            }

            return true;
        }
    }
}
