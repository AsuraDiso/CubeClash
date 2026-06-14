using System.Threading;
using Core.Data;
using Core.Loading;
using Cysharp.Threading.Tasks;

namespace Bootstrap.Loading
{
    public sealed class GameDatabaseLoadingDataPreparer : ILoadingDataPreparer
    {
        private readonly IGameDatabase _gameDatabase;

        public GameDatabaseLoadingDataPreparer(IGameDatabase gameDatabase)
        {
            _gameDatabase = gameDatabase;
        }

        public UniTask PrepareAsync(CancellationToken cancellationToken = default)
        {
            return _gameDatabase.LoadAsync(cancellationToken);
        }
    }
}
