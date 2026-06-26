using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Scripts.Bootstrap.Navigation;
using Game.Scripts.Core.Matchmaking;
using Game.Scripts.Core.Settings;
using UnityEngine;

namespace Game.Features.Matchmaking.Scripts
{
    public sealed class MatchmakingController : IDisposable, IScreenShownHandler, IScreenHiddenHandler
    {
        private readonly MatchmakingView _view;
        private readonly IMatchmakingService _matchmakingService;
        private readonly IHapticsService _haptics;
        private CancellationTokenSource _matchmakingCts;

        public MatchmakingController(MatchmakingView view, IMatchmakingService matchmakingService, IHapticsService haptics)
        {
            _view = view;
            _matchmakingService = matchmakingService;
            _haptics = haptics;

            _matchmakingService.StateChanged += HandleMatchmakingStateChanged;
            _matchmakingService.StatusMessageChanged += HandleStatusMessageChanged;
        }

        public void OnScreenShown() => OnScreenShownAsync().Forget(Debug.LogException);

        private async UniTask OnScreenShownAsync()
        {
            _view.SetStatusText(_matchmakingService.StatusMessage);

            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = new CancellationTokenSource();
            var token = _matchmakingCts.Token;

            await _matchmakingService.PrepareForSearchAsync(token);
            if (token.IsCancellationRequested)
                return;

            _matchmakingService.StartQuickMatchAsync(token).Forget(Debug.LogException);
        }

        public void OnScreenHidden() => CancelMatchmaking();

        public void Dispose()
        {
            _matchmakingService.StateChanged -= HandleMatchmakingStateChanged;
            _matchmakingService.StatusMessageChanged -= HandleStatusMessageChanged;
            CancelMatchmaking();
        }

        private void CancelMatchmaking()
        {
            _matchmakingCts?.Cancel();
            _matchmakingCts?.Dispose();
            _matchmakingCts = null;

            if (_matchmakingService.State == MatchmakingState.InMatch)
                return;

            _matchmakingService.CancelAsync().Forget(Debug.LogException);
        }

        private void HandleMatchmakingStateChanged(MatchmakingState state)
        {
            if (state == MatchmakingState.InMatch)
                _haptics.PlayLight();
        }

        private void HandleStatusMessageChanged(string message) => _view.SetStatusText(message);
    }
}
