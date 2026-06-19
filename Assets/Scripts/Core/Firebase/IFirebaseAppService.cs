using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Firebase
{
    public interface IFirebaseAppService
    {
        public bool IsInitialized { get; }

        public UniTask EnsureInitializedAsync(CancellationToken cancellationToken = default);
    }
}
