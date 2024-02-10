using UnityEngine;

namespace UniLiveViewer
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceService : MonoBehaviour
    {
        AudioSource _audioSource;
        [SerializeField] AudioClip[] _sound;//ボタン音,読み込み音,クリック音

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

        public void PlayOneShot(AudioClip audioClip)
        {
            _audioSource.PlayOneShot(audioClip);
        }
    }
}