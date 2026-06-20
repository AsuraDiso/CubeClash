using UnityEngine;

namespace Bootstrap.Audio
{
    public sealed class AudioSystem : MonoBehaviour
    {
        [field: SerializeField] public AudioSource MusicSource {  get; private set; }
        [field: SerializeField] public AudioSource SfxSource { get; private set; }

    }
}
