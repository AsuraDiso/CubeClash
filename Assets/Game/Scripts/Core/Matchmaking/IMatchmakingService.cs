using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Core.Matchmaking
{
    public interface IMatchmakingService
    {
        public MatchmakingState State { get; }

        public string StatusMessage { get; }

        public event Action<MatchmakingState> StateChanged;
        public event Action<string> StatusMessageChanged;

        public UniTask StartQuickMatchAsync(CancellationToken cancellationToken = default);

        public UniTask PrepareForSearchAsync(CancellationToken cancellationToken = default);

        public UniTask CancelAsync(CancellationToken cancellationToken = default);

        public UniTask ExitMatchAsync(CancellationToken cancellationToken = default);
    }
}
