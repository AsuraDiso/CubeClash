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

        public LoadingSceneEntryPoint(
            IPlayerRepository playerRepository,
            IDeckService deckService,
            ISceneLoaderService sceneLoaderService)
        {
            _playerRepository = playerRepository;
            _deckService = deckService;
            _sceneLoaderService = sceneLoaderService;
        }

        public void Start() => FireAndForget.Run(RunAsync);

        private async UniTask RunAsync()
        {
            await _playerRepository.LoadAsync();
            await _deckService.LoadAsync();
            await _sceneLoaderService.LoadSceneAsync(GameSceneId.MainMenu);
        }
    }
}
