using Fusion;

namespace Infrastructure.Photon
{
    public interface IFusionRunnerAccessor
    {
        public NetworkRunner ActiveRunner { get; }
    }
}
