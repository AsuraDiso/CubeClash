using System;

namespace Game.Scripts.Core.Networking
{
    public sealed class NetworkSessionException : Exception
    {
        public NetworkSessionException(string message) : base(message) { }
    }
}
