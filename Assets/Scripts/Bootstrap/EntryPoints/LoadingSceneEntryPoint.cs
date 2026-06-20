using Bootstrap.Common;
using Core.Data;
using Core.Scenes;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Bootstrap.EntryPoints
{
    public sealed class LoadingSceneEntryPoint : IStartable
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IDeckService _deckService;
        private readonly ISceneLoaderService _sceneLoaderService;
        private readonly ILoadingProgress _loadingProgress;

        public LoadingSceneEntryPoint(
            IPlayerRepository playerRepository,
            IDeckService deckService,
            ISceneLoaderService sceneLoaderService,
            ILoadingProgress loadingProgress)
        {
            _playerRepository = playerRepository;
            _deckService = deckService;
            _sceneLoaderService = sceneLoaderService;
            _loadingProgress = loadingProgress;
        }

        public void Start() => FireAndForget.Run(RunAsync);

        private async UniTask RunAsync()
        {
            await _playerRepository.LoadAsync();
            _loadingProgress.SetPercent(.5f);
            await _deckService.LoadAsync();
            _loadingProgress.SetPercent(1f);
            await _sceneLoaderService.LoadSceneAsync(GameSceneId.MainMenu);
        }
    }
}
