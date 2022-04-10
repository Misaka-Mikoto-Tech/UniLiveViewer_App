using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/DanceInfoData", fileName = "DanceInfoData")]
    public class DanceInfoData : ScriptableObject
    {
        public enum FORMATTYPE
        {
            FBX = 0,
            VMD,
        }

        public bool isReverse = false;
        public string strBeforeName;
        public string viewName;
        public float motionOffsetTime = 0;//現状FBXだけ、VMDはtxtを参照している
        public FORMATTYPE formatType = FORMATTYPE.FBX;
        public AnimationClip baseDanceClip;
        public AnimationClip baseDanceClip_reverse;
        public AnimationClip overrideClip_hand;
        public AnimationClip overrideClip_reverseHand;
        public AnimationClip overrideClip_face;
        public AnimationClip overrideClip_lip;
    }
}