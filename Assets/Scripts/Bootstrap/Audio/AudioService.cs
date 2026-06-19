using Core.Audio;
using UnityEngine;

namespace Bootstrap.Audio
{
    public sealed class AudioService : IAudioService
    {
        readonly IAudioCatalog _catalog;
        readonly AudioSource _musicSource;
        readonly AudioSource _sfxSource;

        float _musicVolume = 1f;
        float _sfxVolume = 1f;
        bool _musicMuted;
        bool _sfxMuted;

        public AudioService(IAudioCatalog catalog)
        {
            _catalog = catalog;

            var root = new GameObject(nameof(AudioService));
            Object.DontDestroyOnLoad(root);

            _musicSource = CreateSource(root, loop: true);
            _sfxSource = CreateSource(root, loop: false);
            ApplyMusicSettings();
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                ApplyMusicSettings();
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        public bool MusicMuted
        {
            get => _musicMuted;
            set
            {
                _musicMuted = value;
                ApplyMusicSettings();
            }
        }

        public bool SfxMuted
        {
            get => _sfxMuted;
            set => _sfxMuted = value;
        }

        public MusicId CurrentMusic { get; private set; } = MusicId.None;

        public void PlayMusic(MusicId id, bool restartIfSame = false)
        {
            if (id == MusicId.None)
            {
                StopMusic();
                return;
            }

            if (!restartIfSame && CurrentMusic == id && _musicSource.isPlaying)
            {
                return;
            }

            if (!_catalog.TryGetMusicClip(id, out var clip))
            {
                return;
            }

            CurrentMusic = id;
            _musicSource.clip = clip;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            CurrentMusic = MusicId.None;
            _musicSource.Stop();
            _musicSource.clip = null;
        }

        public void PlaySfx(SfxId id)
        {
            if (id == SfxId.None || !_catalog.TryGetSfxClip(id, out var clip))
            {
                return;
            }

            _sfxSource.PlayOneShot(clip, EffectiveSfxVolume);
        }

        float EffectiveSfxVolume => _sfxMuted ? 0f : _sfxVolume;

        static AudioSource CreateSource(GameObject host, bool loop)
        {
            var source = host.AddComponent<AudioSource>();
            source.loop = loop;
            source.playOnAwake = false;
            return source;
        }

        void ApplyMusicSettings()
        {
            _musicSource.mute = _musicMuted;
            _musicSource.volume = _musicVolume;
        }
    }
}
