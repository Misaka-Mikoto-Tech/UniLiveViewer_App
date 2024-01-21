using UniLiveViewer.Actor;
using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/CharaInfoData", fileName = "CharaInfoData")]
    public class CharaInfoData : ScriptableObject
    {
        //ほぼ未使用
        public int vrmID = 0;
        public string viewName = "";
        public ActorType ActorType = ActorType.FBX;
        public ExpressionType ExpressionType = ExpressionType.NULL;

        [Header("＜物理設定＞")]
        public Vector3 ColliderCenter = new Vector3(0, 0.8f, 0);
        public float ColliderRadius = 0.25f;
        public float ColliderHeight = 1.5f;
        public CollisionDetectionMode RigidbodyMode = CollisionDetectionMode.ContinuousSpeculative;
        public bool RigidbodyKinematic = true;
        public bool RigidbodyGravity = false;

        [Header("＜接触時の振動(現状VRMのみ)＞")]
        public float power = 0.75f;
        public float time = 0.2f;
    }
}