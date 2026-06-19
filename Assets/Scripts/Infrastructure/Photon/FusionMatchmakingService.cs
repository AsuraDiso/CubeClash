using System;
using System.Threading;
using Core.Data;
using Core.Matchmaking;
using Core.Networking;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Photon
{
    public sealed class FusionMatchmakingService : IMatchmakingService
    {
        private const string QuickMatchSessionName = "CubeClash_QuickMatch";

        private readonly INetworkSession _networkSession;
        private readonly IPlayerRepository _playerRepository;

        public FusionMatchmakingService(INetworkSession networkSession, IPlayerRepository playerRepository)
        {
            _networkSession = networkSession;
            _playerRepository = playerRepository;
            _networkSession.PlayerCountChanged += OnPlayerCountChanged;
        }

        public MatchmakingState State { get; private set; } = MatchmakingState.Idle;

        public string StatusMessage { get; private set; } = "Tap PLAY to find an opponent.";

        public event Action<MatchmakingState> StateChanged;
        public event Action<string> StatusMessageChanged;
        public event Action MatchReady;

        public async UniTask StartQuickMatchAsync(CancellationToken cancellationToken = default)
        {
            if (State is MatchmakingState.Connecting or MatchmakingState.WaitingForOpponent
                or MatchmakingState.InMatch)
            {
                return;
            }

            SetState(MatchmakingState.Connecting, "Connecting to Photon...");

            try
            {
                var profile = _playerRepository.Profile;
                var request = new NetworkSessionRequest(
                    NetworkGameMode.AutoHostOrClient,
                    QuickMatchSessionName,
                    initialScene: null,
                    playerId: profile.PlayerId,
                    displayName: profile.DisplayName);

                await _networkSession.ConnectAsync(request, cancellationToken);

                if (_networkSession.PlayerCount >= MatchmakingConstants.RequiredPlayers)
                {
                    EnterMatch();
                }
                else
                {
                    SetState(MatchmakingState.WaitingForOpponent, FormatWaitingMessage(_networkSession.PlayerCount));
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                SetState(MatchmakingState.Failed, "Could not connect. Try again.");
                throw;
            }
        }

        private void OnPlayerCountChanged(int playerCount)
        {
            if (State == MatchmakingState.WaitingForOpponent)
            {
                SetStatusMessage(FormatWaitingMessage(playerCount));
            }

            if (playerCount >= MatchmakingConstants.RequiredPlayers && State != MatchmakingState.InMatch)
            {
                EnterMatch();
            }
        }

        private void EnterMatch()
        {
            if (State == MatchmakingState.InMatch)
            {
                return;
            }

            SetState(MatchmakingState.InMatch, "Opponent found!");
            Debug.Log("Match ready.");
            MatchReady?.Invoke();
        }

        private static string FormatWaitingMessage(int playerCount) =>
            $"Waiting for opponent ({playerCount}/{MatchmakingConstants.RequiredPlayers})...";

        private void SetState(MatchmakingState newState, string statusMessage)
        {
            SetStatusMessage(statusMessage);

            if (State != newState)
            {
                State = newState;
                StateChanged?.Invoke(newState);
            }
        }

        private void SetStatusMessage(string statusMessage)
        {
            if (StatusMessage == statusMessage)
            {
                return;
            }

            StatusMessage = statusMessage;
            StatusMessageChanged?.Invoke(statusMessage);
        }
    }
}
