using Core.Audio;
using VContainer.Unity;

namespace Bootstrap.Audio
{
    public sealed class SceneMusicStarter : IStartable
    {
        private readonly IAudioService _audioService;
        private readonly SceneMusicBinding _binding;

        public SceneMusicStarter(IAudioService audioService, SceneMusicBinding binding)
        {
            _audioService = audioService;
            _binding = binding;
        }

        public void Start()
        {
            if (_binding.MusicId != MusicId.None) _audioService.PlayMusic(_binding.MusicId);
        }
    }

    public sealed class SceneMusicBinding
    {
        public SceneMusicBinding(MusicId musicId) => MusicId = musicId;

        public MusicId MusicId { get; }
    }
}
