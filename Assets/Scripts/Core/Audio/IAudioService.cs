namespace Core.Audio
{
    public interface IAudioService
    {
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        bool MusicMuted { get; set; }
        bool SfxMuted { get; set; }

        MusicId CurrentMusic { get; }

        void PlayMusic(MusicId id, bool restartIfSame = false);
        void StopMusic();
        void PlaySfx(SfxId id);
    }
}
