using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Data
{
    public interface IPlayerRepository
    {
        bool IsLoaded { get; }

        PlayerProfile Profile { get; }

        UniTask LoadAsync(CancellationToken cancellationToken = default);
    }
}
