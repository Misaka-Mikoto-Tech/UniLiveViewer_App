using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// NOTE: 現状タイトルのみ、消す予定
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceService : MonoBehaviour
    {
        AudioSource _audioSource;
        [SerializeField] AudioClip[] _sound;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;
        }

        public void PlayOneShot(int index)
        {
            if (_sound.Length <= index) return;
            _audioSource.PlayOneShot(_sound[index]);
        }
    }
}