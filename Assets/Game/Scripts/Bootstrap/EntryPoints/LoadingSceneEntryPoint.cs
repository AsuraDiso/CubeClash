using Cysharp.Threading.Tasks;
using Game.Features.Loading.Scripts;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Scenes;
using UnityEngine;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.EntryPoints
{
    public sealed class LoadingSceneEntryPoint : IStartable
    {
        private readonly LoadingView _loadingView;
        private readonly IPlayerRepository _playerRepository;
        private readonly IDeckService _deckService;
        private readonly ISceneLoaderService _sceneLoaderService;

        public LoadingSceneEntryPoint(LoadingView loadingView, IPlayerRepository playerRepository, IDeckService deckService,
            ISceneLoaderService sceneLoaderService)
        {
            _loadingView = loadingView;
            _playerRepository = playerRepository;
            _deckService = deckService;
            _sceneLoaderService = sceneLoaderService;
        }

        public void Start() => RunAsync().Forget(Debug.LogException);

        private async UniTask RunAsync()
        {
            await _playerRepository.LoadAsync();
            _loadingView.SetPercent(0.5f);
            await _deckService.LoadAsync();
            _loadingView.SetPercent(1f);
            await _sceneLoaderService.LoadSceneAsync(GameSceneId.MainMenu);
        }
    }
}
