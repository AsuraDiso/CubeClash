using System;
using System.Threading;
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

        public FusionMatchmakingService(INetworkSession networkSession)
        {
            _networkSession = networkSession;
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
                or MatchmakingState.InMatch) return;

            SetState(MatchmakingState.Connecting, "Connecting to Photon...");

            try
            {
                var request = new NetworkSessionRequest(
                    NetworkGameMode.AutoHostOrClient,
                    QuickMatchSessionName,
                    initialScene: null);

                await _networkSession.ConnectAsync(request, cancellationToken);

                if (_networkSession.PlayerCount >= MatchmakingConstants.RequiredPlayers)
                    EnterMatch();
                else
                    SetState(
                        MatchmakingState.WaitingForOpponent,
                        $"Waiting for opponent ({_networkSession.PlayerCount}/{MatchmakingConstants.RequiredPlayers})...");
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
                var newMsg = $"Waiting for opponent ({playerCount}/{MatchmakingConstants.RequiredPlayers})...";
                if (StatusMessage != newMsg)
                {
                    StatusMessage = newMsg;
                    StatusMessageChanged?.Invoke(newMsg);
                }
            }

            if (playerCount >= MatchmakingConstants.RequiredPlayers && State != MatchmakingState.InMatch) EnterMatch();
        }

        private void EnterMatch()
        {
            if (State == MatchmakingState.InMatch) return;

            SetState(MatchmakingState.InMatch, "Opponent found!");
            Debug.Log("[CubeClash] Match ready.");
            MatchReady?.Invoke();
        }

        private void SetState(MatchmakingState newState, string statusMessage)
        {
            if (StatusMessage != statusMessage)
            {
                StatusMessage = statusMessage;
                StatusMessageChanged?.Invoke(statusMessage);
            }

            if (State != newState)
            {
                State = newState;
                StateChanged?.Invoke(newState);
            }
        }
    }
}
