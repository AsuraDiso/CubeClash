using System;
using System.Threading;
using Bootstrap.UI;
using Bootstrap.UI.Views;
using Core.Matchmaking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Bootstrap.UI.Controllers
{
    public sealed class MainMenuController : IStartable, IDisposable
    {
        private readonly IUiViewFactory _viewFactory;
        private readonly IMatchmakingService _matchmakingService;

        private MainMenuView _view;
        private CancellationTokenSource _matchmakingCts;

        public MainMenuController(IUiViewFactory viewFactory, IMatchmakingService matchmakingService)
        {
            _viewFactory = viewFactory;
            _matchmakingService = matchmakingService;
        }

        public void Start()
        {
            _view = _viewFactory.CreateMainMenuView();
            if (_view == null)
            {
                return;
            }

            _view.PlayClicked += OnPlay;
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.PlayClicked -= OnPlay;
                _viewFactory.Destroy(_view);
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
