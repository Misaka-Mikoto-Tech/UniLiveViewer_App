using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/PresetResourceData", fileName = "PresetResourceData")]
    public class PresetResourceData : ScriptableObject
    {
        public List<DanceInfoData> DanceInfoData;
        public DanceInfoData VMDDanceInfoData;
        public AnimationClip GrabHandAnimationClip;
    }
}