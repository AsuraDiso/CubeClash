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
            _matchmakingService.StateChanged += OnMatchmakingStateChanged;
            _matchmakingService.StatusMessageChanged += OnStatusMessageChanged;
            ApplyState(_matchmakingService.State, _matchmakingService.StatusMessage);
        }

        public void Dispose()
        {
            _matchmakingService.StateChanged -= OnMatchmakingStateChanged;
            _matchmakingService.StatusMessageChanged -= OnStatusMessageChanged;
            _viewFactory.Destroy(_view);
            _view = null;
        }

        private void OnMatchmakingStateChanged(MatchmakingState state) =>
            ApplyState(state, _matchmakingService.StatusMessage);

        private void OnStatusMessageChanged(string statusMessage) => _view.SetStatusText(statusMessage);

        private void ApplyState(MatchmakingState state, string statusMessage)
        {
            var visible = state is MatchmakingState.Connecting
                or MatchmakingState.WaitingForOpponent
                or MatchmakingState.InMatch;

            _view.SetVisible(visible);
            if (visible)
            {
                _view.SetStatusText(statusMessage);
            }
        }
    }
}
