using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Core.Data
{
    public interface IDeckRepository
    {
        public UniTask<DeckPersistenceData> LoadAsync(CancellationToken cancellationToken = default);
        public UniTask SaveAsync(DeckPersistenceData data, CancellationToken cancellationToken = default);
    }
}
