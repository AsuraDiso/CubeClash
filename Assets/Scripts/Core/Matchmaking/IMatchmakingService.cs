using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Matchmaking
{
    public interface IMatchmakingService
    {
        MatchmakingState State { get; }

        string StatusMessage { get; }

        event Action<MatchmakingState> StateChanged;
        event Action<string> StatusMessageChanged;

        event Action MatchReady;

        UniTask StartQuickMatchAsync(CancellationToken cancellationToken = default);
    }
}
