using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Core.Firebase
{
    public interface IFirebaseAppService
    {
        bool IsInitialized { get; }

        UniTask EnsureInitializedAsync(CancellationToken cancellationToken = default);
    }
}
