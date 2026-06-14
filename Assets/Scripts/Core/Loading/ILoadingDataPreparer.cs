using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Loading
{
    public interface ILoadingDataPreparer
    {
        UniTask PrepareAsync(CancellationToken cancellationToken = default);
    }
}
