using System.Collections.Generic;
using Game.Scripts.Core.Battle.Simulation;

namespace Game.Scripts.Core.Battle.Actions
{
    public sealed class BattleActionPipeline
    {
        private readonly Dictionary<string, IBattleActionHandler> _handlers;

        public BattleActionPipeline(IEnumerable<IBattleActionHandler> handlers)
        {
            _handlers = new Dictionary<string, IBattleActionHandler>();
            foreach (var handler in handlers)
                _handlers[handler.TypeId] = handler;
        }

        public bool CanExecute(BattleState state, int actorNetworkId, IBattleAction action) =>
            _handlers.TryGetValue(action.TypeId, out var handler)
            && handler.CanExecute(state, actorNetworkId, action);

        public BattleActionResult TryExecute(BattleState state, int actorNetworkId, IBattleAction action)
        {
            if (!_handlers.TryGetValue(action.TypeId, out var handler))
                return BattleActionResult.Failed;

            return handler.TryExecute(state, actorNetworkId, action);
        }

        public bool HasAnyLegalAction(BattleState state, int actorSlot)
        {
            foreach (var handler in _handlers.Values)
            {
                if (handler.HasAnyLegalAction(state, actorSlot))
                    return true;
            }

            return false;
        }
    }
}
