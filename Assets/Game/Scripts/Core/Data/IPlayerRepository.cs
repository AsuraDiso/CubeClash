using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Core.Data
{
    public interface IPlayerRepository
    {
        public bool IsLoaded { get; }

        public PlayerProfile Profile { get; }

        public event Action ProfileUpdated;

        public UniTask LoadAsync(CancellationToken cancellationToken = default);

        public UniTask UpdateDisplayNameAsync(string displayName, CancellationToken cancellationToken = default);
    }
}
