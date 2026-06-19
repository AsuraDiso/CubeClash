using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Scenes
{
    public interface IBattleSceneLoader
    {
        public UniTask LoadBattleSceneAsync(CancellationToken cancellationToken = default);
    }
}
