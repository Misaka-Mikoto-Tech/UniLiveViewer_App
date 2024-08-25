using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public enum AudioBGM
    {
        // いらないかも
    }

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
        SpotlightSwitch,
        SceneTransition,

        BookClose,
        BookOpen,
        BookPagesFlipping,
        BookPageTurn,
    }

    public enum AudioAmbientMusic
    {
        // TODO
    }

    [CreateAssetMenu(menuName = "MyGame/Audio/AudioClipSettings", fileName = "AudioClipSettings")]
    public class AudioClipSettings : ScriptableObject
    {
        public List<AudioClip> AudioBGM => _presetBGM;
        [SerializeField] List<AudioClip> _presetBGM;

        public List<AudioBGMDataSet> AudioBGMDataSet => _audioBGMDataSet;
        [SerializeField] List<AudioBGMDataSet> _audioBGMDataSet;

        public List<AudioSEDataSet> AudioSEDataSet => _audioSEDataSet;
        [SerializeField] List<AudioSEDataSet> _audioSEDataSet;

        public AudioFootStepsDataSet AudioFootDataSet => _audioFootDataSet;
        [SerializeField] AudioFootStepsDataSet _audioFootDataSet;
    }

    [System.Serializable]
    public class AudioBGMDataSet
    {
        public AudioClip AudioClip => _audioClip;
        [SerializeField] AudioClip _audioClip;
        public AudioBGM AudioType => _audioType;
        [SerializeField] AudioBGM _audioType;
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
    public class AudioFootStepsDataSet
    {
        public IReadOnlyList<AudioClip> AudioClip => _audioClip;
        [SerializeField] List<AudioClip> _audioClip;
    }
}