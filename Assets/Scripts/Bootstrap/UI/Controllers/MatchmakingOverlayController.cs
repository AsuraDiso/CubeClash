using System;
using Bootstrap.UI;
using Bootstrap.UI.Views;
using Core.Matchmaking;
using VContainer.Unity;

namespace Bootstrap.UI.Controllers
{
    public sealed class MatchmakingOverlayController : IStartable, IDisposable
    {
        private readonly IUiViewFactory _viewFactory;
        private readonly IMatchmakingService _matchmakingService;

        private MatchmakingOverlayView _view;

        public MatchmakingOverlayController(IUiViewFactory viewFactory, IMatchmakingService matchmakingService)
        {
            _viewFactory = viewFactory;
            _matchmakingService = matchmakingService;
        }

        public void Start()
        {
            _view = _viewFactory.CreateMatchmakingOverlayView();
            if (_view == null)
            {
                return;
            }

            _matchmakingService.StateChanged += OnMatchmakingStateChanged;
            _matchmakingService.StatusMessageChanged += OnStatusMessageChanged;
            ApplyState(_matchmakingService.State, _matchmakingService.StatusMessage);
        }

        public void Dispose()
        {
            if (_matchmakingService != null)
            {
                _matchmakingService.StateChanged -= OnMatchmakingStateChanged;
                _matchmakingService.StatusMessageChanged -= OnStatusMessageChanged;
            }

            if (_view != null)
            {
                _viewFactory.Destroy(_view);
                _view = null;
            }
        }

        private void OnMatchmakingStateChanged(MatchmakingState state)
        {
            ApplyState(state, _matchmakingService.StatusMessage);
        }

        private void OnStatusMessageChanged(string statusMessage)
        {
            if (_view != null)
            {
                _view.SetStatusText(statusMessage);
            }
        }

        private void ApplyState(MatchmakingState state, string statusMessage)
        {
            if (_view == null)
            {
                return;
            }

            switch (state)
            {
                case MatchmakingState.Connecting:
                case MatchmakingState.WaitingForOpponent:
                case MatchmakingState.InMatch:
                    _view.SetVisible(true);
                    _view.SetStatusText(statusMessage);
                    break;
                default:
                    _view.SetVisible(false);
                    break;
            }
        }
    }
}
