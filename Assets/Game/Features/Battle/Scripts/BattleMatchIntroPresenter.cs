using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Core.Battle;

namespace Game.Features.Battle.Scripts
{
    public sealed class BattleMatchIntroPresenter
    {
        public async UniTask PlayAsync(BattleMatchIntroView introView, IBattleGateway gateway, CancellationToken cancellationToken)
        {
            if (introView == null || gateway == null)
                return;

            introView.BindPlayers(
                gateway.LocalProfile.DisplayName,
                gateway.OpponentProfile.DisplayName);

            var sequence = introView.PlayIntroSequence();
            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => sequence.Kill());

            await sequence.AsyncWaitForCompletion();
        }
    }
}
