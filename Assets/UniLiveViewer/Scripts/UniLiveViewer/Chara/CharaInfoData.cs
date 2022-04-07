using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/CharaInfoData", fileName = "CharaInfoData")]
    public class CharaInfoData : ScriptableObject
    {
        public enum CHARATYPE
        {
            NULL = 0,
            UnityChan,//UV����
            CandyChan,//UV����
            UnityChanSSU,//Bone����
            UnityChanSD,//Bone����
            VketChan,///BlendShape����
            UnityChanKAGURA,//Bone����
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

        [Header("���ڐG���̐U��(����VRM�̂�)��")]
        public float power = 0.75f;
        public float time = 0.2f;
    }
}