using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/ActorOptionSetting", fileName = "ActorOptionSetting")]
    public class ActorOptionSetting : ScriptableObject
    {
        public IReadOnlyList<AudioClip> FootstepAudioClips => _footstepAudioClips;
        [SerializeField] List<AudioClip> _footstepAudioClips;
    }

    // TODO: 雑、オーディオマネ作る
    public static class FootstepAudio 
    {
        public static bool IsFootstepAudio { get; private set; }

        static FootstepAudio()
        {
            IsFootstepAudio = FileReadAndWriteUtility.UserProfile.StepSE;
        }

        public static void SetEnable(bool isEnable)
        {
            IsFootstepAudio = isEnable;
        }
    }
}