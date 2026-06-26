using Game.Features.AppBootstrap.Scripts;
using Game.Scripts.Core.Audio;
using UnityEngine;

namespace Game.Scripts.Bootstrap.Audio
{
    public sealed class AudioService : IAudioService
    {
        private readonly AudioCatalog _catalog;
        private readonly AudioSource _musicSource;
        private readonly AudioSource _sfxSource;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _musicMuted;
        private bool _sfxMuted;

        public AudioService(AudioCatalog catalog, AudioSystem audioSystemPrefab)
        {
            _catalog = catalog;

            var audioSystem = Object.Instantiate(audioSystemPrefab);
            Object.DontDestroyOnLoad(audioSystem.gameObject);

            _musicSource = audioSystem.MusicSource;
            _sfxSource = audioSystem.SfxSource;
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
                return;

            if (!_catalog.TryGetMusicClip(id, out var clip))
                return;

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
                return;

            _sfxSource.PlayOneShot(clip, EffectiveSfxVolume);
        }

        private float EffectiveSfxVolume => _sfxMuted ? 0f : _sfxVolume;

        private void ApplyMusicSettings()
        {
            _musicSource.mute = _musicMuted;
            _musicSource.volume = _musicVolume;
        }
    }
}
