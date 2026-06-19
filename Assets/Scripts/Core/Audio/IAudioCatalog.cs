using UnityEngine;

namespace Core.Audio
{
    public interface IAudioCatalog
    {
        public bool TryGetMusicClip(MusicId id, out AudioClip clip);
        public bool TryGetSfxClip(SfxId id, out AudioClip clip);
    }
}
