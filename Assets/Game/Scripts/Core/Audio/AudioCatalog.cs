using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Core.Audio
{
    [CreateAssetMenu(menuName = "CubeClash/Audio Catalog", fileName = "AudioCatalog")]
    public sealed class AudioCatalog : ScriptableObject
    {
        [SerializeField] private MusicEntry[] _music = Array.Empty<MusicEntry>();
        [SerializeField] private SfxEntry[] _sfx = Array.Empty<SfxEntry>();

        private Dictionary<MusicId, AudioClip> _musicLookup;
        private Dictionary<SfxId, AudioClip> _sfxLookup;

        public bool TryGetMusicClip(MusicId id, out AudioClip clip)
        {
            EnsureBuilt();
            return _musicLookup.TryGetValue(id, out clip);
        }

        public bool TryGetSfxClip(SfxId id, out AudioClip clip)
        {
            EnsureBuilt();
            return _sfxLookup.TryGetValue(id, out clip);
        }

        private void EnsureBuilt()
        {
            if (_musicLookup != null)
                return;

            _musicLookup = new Dictionary<MusicId, AudioClip>(_music.Length);
            foreach (var entry in _music)
            {
                if (entry.Id == MusicId.None || entry.Clip == null)
                    continue;

                _musicLookup[entry.Id] = entry.Clip;
            }

            _sfxLookup = new Dictionary<SfxId, AudioClip>(_sfx.Length);
            foreach (var entry in _sfx)
            {
                if (entry.Id == SfxId.None || entry.Clip == null)
                    continue;

                _sfxLookup[entry.Id] = entry.Clip;
            }
        }

        private void OnEnable() => ResetLookup();
        private void OnDisable() => ResetLookup();

        private void ResetLookup()
        {
            _musicLookup = null;
            _sfxLookup = null;
        }

        [Serializable]
        public struct MusicEntry
        {
            public MusicId Id;
            public AudioClip Clip;
        }

        [Serializable]
        public struct SfxEntry
        {
            public SfxId Id;
            public AudioClip Clip;
        }
    }
}
