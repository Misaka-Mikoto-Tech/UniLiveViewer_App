using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using System.Linq;

namespace UniLiveViewer
{
    public enum AudioSE
    {
        ButtonClick,
        TabClick,
        SpringMenuItem,
        MenuOpen,
        MenuClose,
        ObjectDelete,
        AttachSuccess,
        ChangeItemColor,
        ActorGrab,
        ActorRelease,
        ActorRotation,
        ActorSummon,
        SpotlightSwitch,
        SceneTransition,

        BookClose,
        BookOpen,
        BookPagesFlipping,
        BookPageTurn,
    }

    public enum AudioHandPsylliumSE
    {
        Default,
        Thunder,
        Wind,
        Water,
        Darkness,
        Flame,
        Light
    }

    [CreateAssetMenu(menuName = "MyGame/Audio/AudioClipSettings", fileName = "AudioClipSettings")]
    public class AudioClipSettings : ScriptableObject
    {
        public List<AudioClip> AudioBGM => _presetBGM;
        [SerializeField] List<AudioClip> _presetBGM;

        public List<AudioSEDataSet> AudioSEDataSet => _audioSEDataSet;
        [SerializeField] List<AudioSEDataSet> _audioSEDataSet;

        public List<AudioHandPsylliumDataSet> AudioHandPsylliumDataSet => _audioHandPsylliumDataSet;
        [SerializeField] List<AudioHandPsylliumDataSet> _audioHandPsylliumDataSet;

        public SceneAudioDataSet GetSceneAudioDataSet(SceneType sceneType)
            => _sceneAudioDataSet?.FirstOrDefault(x => x.SceneType == sceneType);
        [SerializeField] SceneAudioDataSet[] _sceneAudioDataSet;
    }

    [System.Serializable]
    public class AudioSEDataSet
    {
        public AudioClip AudioClip => _audioClip;
        [SerializeField] AudioClip _audioClip;
        public AudioSE AudioType => _audioType;
        [SerializeField] AudioSE _audioType;
    }

    [System.Serializable]
    public class AudioHandPsylliumDataSet
    {
        public AudioClip AudioClip => _audioClip;
        [SerializeField] AudioClip _audioClip;
        public AudioHandPsylliumSE AudioType => _audioType;
        [SerializeField] AudioHandPsylliumSE _audioType;
    }

    [System.Serializable]
    public class SceneAudioDataSet
    {
        public SceneType SceneType => _sceneType;
        [SerializeField] SceneType _sceneType;

        public AudioClip AmbientSoundAudioClip => _ambientSoundAudioClip;
        [SerializeField] AudioClip _ambientSoundAudioClip;

        public AudioFootStepsDataSet AudioFootStepsDataSet => _audioFootStepsDataSet;
        [SerializeField] AudioFootStepsDataSet _audioFootStepsDataSet;
    }

    [System.Serializable]
    public class AudioFootStepsDataSet
    {
        public IReadOnlyList<AudioClip> AudioClip => _audioClip;
        [SerializeField] List<AudioClip> _audioClip;
    }
}