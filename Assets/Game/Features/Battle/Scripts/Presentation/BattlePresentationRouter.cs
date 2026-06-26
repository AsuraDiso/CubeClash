using System.Collections.Generic;
using Game.Scripts.Core.Battle;

namespace Game.Features.Battle.Scripts.Presentation
{
    public sealed class BattlePresentationRouter
    {
        private readonly IReadOnlyList<IBattleActionPresentationHandler> _handlers;
        private readonly Dictionary<string, IBattleActionPresentationHandler> _handlerByType = new();

        public BattlePresentationRouter(IEnumerable<IBattleActionPresentationHandler> handlers)
        {
            _handlers = new List<IBattleActionPresentationHandler>(handlers);
            foreach (var handler in _handlers)
                _handlerByType[handler.TypeId] = handler;
        }

        public void WireAll(BattlePresentationContext context)
        {
            foreach (var handler in _handlers)
                handler.Wire(context);
        }

        public void UnwireAll(BattlePresentationContext context)
        {
            foreach (var handler in _handlers)
                handler.Unwire(context);
        }

        public void OnActionResolved(BattlePresentationContext context, BattleActionResolvedEventArgs args)
        {
            if (_handlerByType.TryGetValue(args.Action.TypeId, out var handler))
                handler.OnActionResolved(context, args);
        }

        public void OnActionFailed(BattlePresentationContext context)
        {
            foreach (var handler in _handlers)
                handler.OnActionFailed(context);
        }
    }
}
