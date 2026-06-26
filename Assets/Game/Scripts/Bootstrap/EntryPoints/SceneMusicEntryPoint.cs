using Game.Scripts.Core.Audio;
using VContainer.Unity;

namespace Game.Scripts.Bootstrap.EntryPoints
{
    public sealed class SceneMusicEntryPoint : IStartable
    {
        private readonly IAudioService _audioService;
        private readonly MusicId _musicId;

        public SceneMusicEntryPoint(IAudioService audioService, MusicId musicId)
        {
            _audioService = audioService;
            _musicId = musicId;
        }

        public void Start() => _audioService.PlayMusic(_musicId);
    }
}
