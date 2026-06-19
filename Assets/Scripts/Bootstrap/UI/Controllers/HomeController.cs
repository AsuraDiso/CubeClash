using System;
using System.Threading;
using Bootstrap.Common;
using Bootstrap.UI.Views;
using Core.Matchmaking;
using Cysharp.Threading.Tasks;

namespace Bootstrap.UI.Controllers
{
    public sealed class HomeController : IDisposable
    {
        private readonly IMatchmakingService _matchmakingService;

        private HomeView _view;
        private CancellationTokenSource _matchmakingCts;

        public HomeController(IMatchmakingService matchmakingService)
        {
            _matchmakingService = matchmakingService;
        }

        public void Bind(HomeView view) =>
            ViewBinding.Rebind(ref _view, view, v => v.PlayClicked += OnPlay, v => v.PlayClicked -= OnPlay);

        public void Dispose()
        {
            ViewBinding.Unbind(ref _view, v => v.PlayClicked -= OnPlay);
            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = null;
        }

        private void OnPlay()
        {
            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = new CancellationTokenSource();
            FireAndForget.Run(StartMatchAsync, _matchmakingCts.Token);
        }

        private UniTask StartMatchAsync(CancellationToken cancellationToken) =>
            _matchmakingService.StartQuickMatchAsync(cancellationToken);
    }
}
