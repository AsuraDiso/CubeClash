using System;
using System.Threading;
using Bootstrap.UI.Views;
using Core.Matchmaking;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        public void Bind(HomeView view)
        {
            if (_view != null)
            {
                _view.PlayClicked -= OnPlay;
            }

            _view = view;

            if (_view != null)
            {
                _view.PlayClicked += OnPlay;
            }
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.PlayClicked -= OnPlay;
                _view = null;
            }

            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = null;
        }

        private void OnPlay()
        {
            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = new CancellationTokenSource();
            StartMatchAsync(_matchmakingCts.Token).Forget();
        }

        private async UniTask StartMatchAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _matchmakingService.StartQuickMatchAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
