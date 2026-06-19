using UnityEngine;

namespace Core.Audio
{
    public interface IAudioCatalog
    {
        bool TryGetMusicClip(MusicId id, out AudioClip clip);
        bool TryGetSfxClip(SfxId id, out AudioClip clip);
    }
}
