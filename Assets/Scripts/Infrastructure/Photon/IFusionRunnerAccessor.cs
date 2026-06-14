using Fusion;

namespace Infrastructure.Photon
{
    public interface IFusionRunnerAccessor
    {
        NetworkRunner ActiveRunner { get; }
    }
}
