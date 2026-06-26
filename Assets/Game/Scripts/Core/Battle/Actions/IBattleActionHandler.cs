using Game.Scripts.Core.Battle.Simulation;

namespace Game.Scripts.Core.Battle.Actions
{
    public interface IBattleActionHandler
    {
        string TypeId { get; }

        bool CanExecute(BattleState state, int actorNetworkId, IBattleAction action);

        BattleActionResult TryExecute(BattleState state, int actorNetworkId, IBattleAction action);

        bool HasAnyLegalAction(BattleState state, int actorSlot);
    }
}
