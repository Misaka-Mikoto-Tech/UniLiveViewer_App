using System.Linq;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer
{
    /// <summary>
    /// AudioSourceの統括
    /// ミキサー検証済んだら置き換えるかも
    /// </summary>
    public class RootAudioSourceService : MonoBehaviour
    {
        [SerializeField] AudioSource _bgmAudioSource;
        [SerializeField] AudioSource[] _seAudioSources;
        [SerializeField] AudioSource _ambientAudioSources;

        /// <summary> 0~1.0f </summary>
        public float MasterVolumeRate { get; private set; }
        /// <summary> 0~1.0f </summary>
        public float BGMVolumeRate { get; private set; }
        /// <summary> 0~1.0f </summary>
        public float SEVolumeRate { get; private set; }
        /// <summary> 0~1.0f </summary>
        public float AmbientVolumeRate { get; private set; }
        /// <summary> 0~1.0f </summary>
        public IReadOnlyReactiveProperty<float> FootStepsVolumeRate => _footStepsVolumeRate;
        readonly ReactiveProperty<float> _footStepsVolumeRate = new();
        public float _preFootStepsVolumeRate;

        int _currentSE = 0;
        AudioClipSettings _audioClipSettings;

        [Inject]
        public void Construct(AudioClipSettings audioClipSettings)
        {
            _audioClipSettings = audioClipSettings;
        }

        void Awake()
        {
            MasterVolumeRate = FileReadAndWriteUtility.UserProfile.SoundMaster * 0.01f;
            BGMVolumeRate = FileReadAndWriteUtility.UserProfile.SoundBGM * 0.01f;
            SEVolumeRate = FileReadAndWriteUtility.UserProfile.SoundSE * 0.01f;
            AmbientVolumeRate = FileReadAndWriteUtility.UserProfile.SoundAmbient * 0.01f;
            _preFootStepsVolumeRate = FileReadAndWriteUtility.UserProfile.SoundFootSteps * 0.01f;
            _footStepsVolumeRate.Value = _preFootStepsVolumeRate;

            _bgmAudioSource.volume = BGMVolumeRate * MasterVolumeRate;
            foreach (var audioSource in _seAudioSources)
            {
                audioSource.volume = SEVolumeRate * MasterVolumeRate;
            }
            _ambientAudioSources.volume = AmbientVolumeRate * MasterVolumeRate;
        }

        public void Start()
        {
            PlayOneShotAmbientAudio();
        }

        /// <param name="volume">0~100</param>
        public void SetMasterVolume(float volume)
        {
            MasterVolumeRate = volume * 0.01f;
            FileReadAndWriteUtility.UserProfile.SoundMaster = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _bgmAudioSource.volume = BGMVolumeRate * MasterVolumeRate;
            foreach (var audioSource in _seAudioSources)
            {
                audioSource.volume = SEVolumeRate * MasterVolumeRate;
            }
            _ambientAudioSources.volume = AmbientVolumeRate * MasterVolumeRate;
            _footStepsVolumeRate.Value = _preFootStepsVolumeRate * MasterVolumeRate;
        }

        /// <param name="volume">0~100</param>
        public void SetBGMVolume(float volume)
        {
            BGMVolumeRate = volume * 0.01f;
            _bgmAudioSource.volume = BGMVolumeRate * MasterVolumeRate;

            FileReadAndWriteUtility.UserProfile.SoundBGM = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <param name="volume">0~100</param>
        public void SetSEVolume(float volume)
        {
            SEVolumeRate = volume * 0.01f;
            foreach (var audioSource in _seAudioSources)
            {
                audioSource.volume = SEVolumeRate * MasterVolumeRate;
            }

            FileReadAndWriteUtility.UserProfile.SoundSE = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <param name="volume">0~100</param>
        public void SetAmbientVolume(float volume)
        {
            AmbientVolumeRate = volume * 0.01f;
            _ambientAudioSources.volume = AmbientVolumeRate * MasterVolumeRate;

            FileReadAndWriteUtility.UserProfile.SoundAmbient = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <param name="volume">0~100</param>
        public void SetFootStepsVolume(float volume)
        {
            _preFootStepsVolumeRate = volume * 0.01f;
            _footStepsVolumeRate.Value = _preFootStepsVolumeRate * MasterVolumeRate;

            FileReadAndWriteUtility.UserProfile.SoundFootSteps = volume;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        public void PlayOneShot(AudioSE audioType)
        {
            var clip = _audioClipSettings.AudioSEDataSet.FirstOrDefault(x => x.AudioType == audioType).AudioClip;
            GetCurrentAudioSource().PlayOneShot(clip);
        }

        // TODO: ambient
        public void PlayOneShotAmbientAudio()
        {
            var clip = _audioClipSettings.GetSceneAudioDataSet(SceneChangeService.GetSceneType).AmbientSoundAudioClip;
            _ambientAudioSources.clip = clip;
            _ambientAudioSources.loop = true;
            _ambientAudioSources.Play();
        }

        AudioSource GetCurrentAudioSource()
        {
            _currentSE++;
            if (_currentSE <= _seAudioSources.Length) _currentSE = 0;
            return _seAudioSources[_currentSE];
        }
    }
}