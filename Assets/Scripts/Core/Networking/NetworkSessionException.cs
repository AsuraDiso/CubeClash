using System;

namespace Core.Networking
{
    public sealed class NetworkSessionException : Exception
    {
        public string ShutdownReason { get; }

        public NetworkSessionException(string shutdownReason, string message)
            : base(message) =>
            ShutdownReason = shutdownReason;
    }
}
