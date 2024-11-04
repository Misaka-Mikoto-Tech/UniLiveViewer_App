using UnityEngine;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 足音用
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceService : MonoBehaviour
    {
        AudioSource _audioSource;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = 1;
        }

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        public void PlayOneShot(AudioClip audioClip)
        {
            _audioSource.PlayOneShot(audioClip);
        }
    }
}