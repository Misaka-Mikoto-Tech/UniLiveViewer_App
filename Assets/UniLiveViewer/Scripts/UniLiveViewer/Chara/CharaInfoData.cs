using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/CharaInfoData", fileName = "CharaInfoData")]
    public class CharaInfoData : ScriptableObject
    {
        public enum CHARATYPE
        {
            NULL = 0,
            UnityChan,//UV方式
            CandyChan,//UV方式
            UnityChanSSU,//Bone方式
            UnityChanSD,//Bone方式
            VketChan,///BlendShape方式
            UnityChanKAGURA,//Bone方式
            VRM_UV,
            VRM_Bone,
            VRM_BlendShape,
        }
        public enum FORMATTYPE
        {
            FBX = 0,
            VRM,
        }

        public int vrmID = 0;
        public string viewName = "";
        public FORMATTYPE formatType = FORMATTYPE.FBX;
        public CHARATYPE charaType = CHARATYPE.NULL;

        [Header("＜接触時の振動(現状VRMのみ)＞")]
        public float power = 0.75f;
        public float time = 0.2f;
    }
}