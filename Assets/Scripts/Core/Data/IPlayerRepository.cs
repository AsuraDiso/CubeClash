using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Data
{
    public interface IPlayerRepository
    {
        public bool IsLoaded { get; }

        public PlayerProfile Profile { get; }

        public UniTask LoadAsync(CancellationToken cancellationToken = default);
    }
}
