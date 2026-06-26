using Game.Scripts.Core.Battle.Actions;

namespace Game.Scripts.Core.Battle.Actions.Card
{
    public sealed class CardBattleActionResult : BattleActionResult
    {
        public int DeckCardIndex { get; }
        public int Damage { get; }
        public int[] ConsumedDieIndices { get; }

        public CardBattleActionResult(int actorNetworkId, string actorDisplayName, int deckCardIndex, int damage,
            int[] consumedDieIndices)
            : base(true, actorNetworkId, actorDisplayName)
        {
            DeckCardIndex = deckCardIndex;
            Damage = damage;
            ConsumedDieIndices = consumedDieIndices;
        }
    }
}
