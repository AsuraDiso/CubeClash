using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Matchmaking
{
    public interface IMatchmakingService
    {
        public MatchmakingState State { get; }

        public string StatusMessage { get; }

        public event Action<MatchmakingState> StateChanged;
        public event Action<string> StatusMessageChanged;

        public event Action MatchReady;

        public UniTask StartQuickMatchAsync(CancellationToken cancellationToken = default);
    }
}
