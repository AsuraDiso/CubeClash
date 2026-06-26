using Game.Scripts.Core.Battle.Actions;

namespace Game.Scripts.Core.Battle
{
    public sealed class BattleActionResolvedEventArgs
    {
        public IBattleAction Action { get; }
        public BattleActionResult Result { get; }
        public bool IsLocalActor { get; }

        public BattleActionResolvedEventArgs(IBattleAction action, BattleActionResult result, bool isLocalActor)
        {
            Action = action;
            Result = result;
            IsLocalActor = isLocalActor;
        }
    }
}
