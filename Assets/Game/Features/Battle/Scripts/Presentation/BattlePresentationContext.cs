using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Battle.Actions;

namespace Game.Features.Battle.Scripts.Presentation
{
    public sealed class BattlePresentationContext
    {
        public BattleView View { get; }
        public IBattleGateway Gateway { get; }
        public bool ResolveInFlight { get; set; }
        public bool IsGameOver { get; set; }
        public bool IntroComplete { get; set; }

        public BattlePresentationContext(BattleView view, IBattleGateway gateway)
        {
            View = view;
            Gateway = gateway;
        }

        public void SubmitAction(IBattleAction action) => Gateway.SubmitAction(action);

        public bool CanSubmitAction =>
            IntroComplete && !IsGameOver && !ResolveInFlight && Gateway.IsMatchReady && Gateway.IsMyTurn;
    }
}
