namespace Game.Scripts.Core.Audio
{
    public interface IAudioService
    {
        public float MusicVolume { get; set; }
        public float SfxVolume { get; set; }
        public bool MusicMuted { get; set; }
        public bool SfxMuted { get; set; }

        public MusicId CurrentMusic { get; }

        public void PlayMusic(MusicId id, bool restartIfSame = false);
        public void StopMusic();
        public void PlaySfx(SfxId id);
    }
}
