using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Data
{
    public interface IGameDatabase
    {
        bool IsLoaded { get; }

        PlayerProfile Player { get; }

        GameBalanceData Balance { get; }

        UniTask LoadAsync(CancellationToken cancellationToken = default);
    }
}
