using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Networking
{
    public interface INetworkSession
    {
        NetworkSessionState State { get; }

        int PlayerCount { get; }

        event Action<int> PlayerCountChanged;

        UniTask ConnectAsync(NetworkSessionRequest request, CancellationToken cancellationToken = default);

        UniTask DisconnectAsync(CancellationToken cancellationToken = default);
    }
}
