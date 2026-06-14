using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Scenes
{
    public interface IBattleSceneLoader
    {
        UniTask LoadBattleSceneAsync(CancellationToken cancellationToken = default);
    }
}
