using System;
using System.Threading;
using Bootstrap.Common;
using Core.Battle;
using Core.Matchmaking;
using Core.Scenes;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Bootstrap.Flow
{
    public sealed class MatchFlowCoordinator : IStartable, IDisposable
    {
        private readonly IMatchmakingService _matchmakingService;
        private readonly IBattleSceneLoader _battleSceneLoader;
        private readonly IBattleSessionSpawner _battleSessionSpawner;
        private int _isEnteringBattle;

        public MatchFlowCoordinator(
            IMatchmakingService matchmakingService,
            IBattleSceneLoader battleSceneLoader,
            IBattleSessionSpawner battleSessionSpawner)
        {
            _matchmakingService = matchmakingService;
            _battleSceneLoader = battleSceneLoader;
            _battleSessionSpawner = battleSessionSpawner;
        }

        public void Start() => _matchmakingService.MatchReady += OnMatchReady;

        public void Dispose() => _matchmakingService.MatchReady -= OnMatchReady;

        private void OnMatchReady() => FireAndForget.Run(EnterBattleAsync);

        private async UniTask EnterBattleAsync()
        {
            if (Interlocked.CompareExchange(ref _isEnteringBattle, 1, 0) != 0)
            {
                return;
            }

            try
            {
                await _battleSceneLoader.LoadBattleSceneAsync();
                _battleSessionSpawner.Start();
            }
            finally
            {
                Interlocked.Exchange(ref _isEnteringBattle, 0);
            }
        }
    }
}
