using Game.Scripts.Core.Battle;

namespace Game.Features.Battle.Scripts.Presentation
{
    public interface IBattleActionPresentationHandler
    {
        string TypeId { get; }

        void Wire(BattlePresentationContext context);

        void Unwire(BattlePresentationContext context);

        void OnActionResolved(BattlePresentationContext context, BattleActionResolvedEventArgs args);

        void OnActionFailed(BattlePresentationContext context);
    }
}
