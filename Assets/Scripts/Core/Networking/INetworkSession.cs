using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core.Networking
{
    public interface INetworkSession
    {
        public NetworkSessionState State { get; }

        public int PlayerCount { get; }

        public event Action<int> PlayerCountChanged;

        public UniTask ConnectAsync(NetworkSessionRequest request, CancellationToken cancellationToken = default);

        public UniTask DisconnectAsync(CancellationToken cancellationToken = default);
    }
}
