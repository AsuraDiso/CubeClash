using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Game.Features.Network.Scripts;
using Game.Scripts.Core.Battle;
using Game.Scripts.Core.Networking;
using UnityEngine;
using Game.Scripts.Infrastructure.Battle.Session;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Game.Scripts.Infrastructure.Networking
{
    public sealed class FusionNetworkSessionService : INetworkSession, INetworkRunnerCallbacks, IDisposable
    {
        private readonly GamePrefabCatalog _prefabCatalog;
        private readonly IBattleControllerRegistry _battleControllerRegistry;

        private NetworkRunner _runner;

        public FusionNetworkSessionService(GamePrefabCatalog prefabCatalog, IBattleControllerRegistry battleControllerRegistry)
        {
            _prefabCatalog = prefabCatalog;
            _battleControllerRegistry = battleControllerRegistry;
        }

        public NetworkSessionState State { get; private set; } = NetworkSessionState.Disconnected;

        public int PlayerCount => _runner == null ? 0 : _runner.ActivePlayers.Count();

        public NetworkRunner ActiveRunner => _runner;

        public event Action<int> PlayerCountChanged;
        public event Action<NetworkSessionState> StateChanged;

        public async UniTask ConnectAsync(NetworkSessionRequest request, CancellationToken cancellationToken = default)
        {
            ValidateCanConnect();
            SetState(NetworkSessionState.Connecting);

            try
            {
                await StartSessionAsync(request, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await FailConnectAsync();
                throw;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.LogException(exception);
                await FailConnectAsync();
                throw;
            }
        }

        public async UniTask DisconnectAsync(CancellationToken cancellationToken = default)
        {
            if (State is NetworkSessionState.Disconnected or NetworkSessionState.Disconnecting)
                return;

            SetState(NetworkSessionState.Disconnecting);
            await CleanupRunnerAsync();
            cancellationToken.ThrowIfCancellationRequested();
            SetState(NetworkSessionState.Disconnected);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (_runner == runner)
                _runner = null;

            SetState(NetworkSessionState.Disconnected);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => NotifyPlayerCountChanged();

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => NotifyPlayerCountChanged();

        public void Dispose()
        {
            var runner = DetachRunner();
            if (runner != null)
                ShutdownRunnerAsync(runner).Forget(Debug.LogException);
        }

        private static async UniTask ShutdownRunnerAsync(NetworkRunner runner)
        {
            if (runner.IsRunning)
                await runner.Shutdown();

            Object.Destroy(runner.gameObject);
        }

        private void ValidateCanConnect()
        {
            if (State is NetworkSessionState.Connecting or NetworkSessionState.Connected)
                throw new InvalidOperationException($"Cannot connect while session state is {State}.");
        }

        private async UniTask StartSessionAsync(NetworkSessionRequest request, CancellationToken cancellationToken)
        {
            var setup = InitializeRunner(request);
            var result = await _runner.StartGame(BuildStartGameArgs(request, setup));

            cancellationToken.ThrowIfCancellationRequested();
            await CompleteConnectionOrThrowAsync(result);
        }

        private FusionNetworkRunnerSetup InitializeRunner(NetworkSessionRequest request)
        {
            var payload = request.SessionPayload as BattleSessionPayload;
            var runnerSetup = CreateRunnerSetup();
            _runner = runnerSetup.Runner;

            if (runnerSetup.SessionBridge != null && payload != null)
                runnerSetup.SessionBridge.Initialize(_battleControllerRegistry, payload);

            _runner.AddCallbacks(this);
            return runnerSetup;
        }

        private FusionNetworkRunnerSetup CreateRunnerSetup()
        {
            var runner = Object.Instantiate(_prefabCatalog.NetworkRunnerPrefab);
            Object.DontDestroyOnLoad(runner.gameObject);

            if (!runner.TryGetComponent(out FusionNetworkRunnerSetup setup))
                throw new InvalidOperationException("NetworkRunner prefab is missing FusionNetworkRunnerSetup.");

            return setup;
        }

        private static NetworkSceneInfo BuildSceneInfo(NetworkSessionRequest request)
        {
            var sceneInfo = new NetworkSceneInfo();
            if (!request.InitialScene.HasValue)
                return sceneInfo;

            var sceneRef = SceneRef.FromIndex((int)request.InitialScene.Value);
            if (sceneRef.IsValid)
                sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);

            return sceneInfo;
        }

        private static StartGameArgs BuildStartGameArgs(NetworkSessionRequest request, FusionNetworkRunnerSetup setup)
        {
            var startGameArgs = new StartGameArgs
            {
                GameMode = FusionGameModeMapper.ToFusion(request.GameMode),
                SessionName = request.SessionName,
                Scene = BuildSceneInfo(request),
                SceneManager = setup.SceneManager,
                ObjectProvider = setup.ObjectProvider,
                ConnectionToken = ProfileConnectionToken.Encode(request.PlayerId, request.DisplayName)
            };

            if (request.MaxPlayers.HasValue)
                startGameArgs.PlayerCount = request.MaxPlayers.Value;

            return startGameArgs;
        }

        private async UniTask CompleteConnectionOrThrowAsync(StartGameResult result)
        {
            if (!result.Ok)
            {
                await CleanupRunnerAsync();
                SetState(NetworkSessionState.Disconnected);
                throw new NetworkSessionException(
                    $"Failed to start Fusion session: {result.ShutdownReason}");
            }

            SetState(NetworkSessionState.Connected);
            NotifyPlayerCountChanged();
        }

        private async UniTask FailConnectAsync()
        {
            await CleanupRunnerAsync();
            SetState(NetworkSessionState.Disconnected);
        }

        private async UniTask CleanupRunnerAsync()
        {
            var runner = DetachRunner();
            if (runner == null)
                return;

            if (runner.IsRunning) await runner.Shutdown();

            UnityEngine.Object.Destroy(runner.gameObject);
        }

        private NetworkRunner DetachRunner()
        {
            if (_runner == null)
                return null;

            var runner = _runner;
            _runner = null;
            runner.RemoveCallbacks(this);
            return runner;
        }

        private void NotifyPlayerCountChanged() => PlayerCountChanged?.Invoke(PlayerCount);

        private void SetState(NetworkSessionState newState)
        {
            if (State == newState)
                return;

            State = newState;
            StateChanged?.Invoke(newState);
        }

        void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            if (_runner == runner)
                SetState(NetworkSessionState.Disconnected);
        }
        void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
        void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    }
}
