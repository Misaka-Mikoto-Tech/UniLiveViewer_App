using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/CharaInfoData", fileName = "CharaInfoData")]
    public class CharaInfoData : ScriptableObject
    {
        public enum CHARATYPE
        {
            NULL = 0,
            UnityChan,//UV•û®
            CandyChan,//UV•û®
            UnityChanSSU,//Bone•û®
            UnityChanSD,//Bone•û®
            VketChan,///BlendShape•û®
            UnityChanKAGURA,//Bone•û®
            VRM_UV,
            VRM_Bone,
            VRM_BlendShape,
        }
        public enum FORMATTYPE
        {
            FBX = 0,
            VRM,
        }

        public string viewName = "";
        public FORMATTYPE formatType = FORMATTYPE.FBX;
        public CHARATYPE charaType = CHARATYPE.NULL;

        [Header("ƒÚG‚ÌU“®(Œ»óVRM‚Ì‚İ)„")]
        public float power = 0.75f;
        public float time = 0.2f;
    }
}