using NanaCiel;
using UnityEngine;


namespace UniLiveViewer 
{
    [RequireComponent(typeof(NormalizedBoneGenerator))]
    public abstract class LookAtBase : MonoBehaviour
    {
        [Header("＜共有(自動管理)＞")]
        public NormalizedBoneGenerator test;

        protected Transform _lookTarget;
        protected Animator _animator;
        protected CharaController _charaCon;

        protected float _searchAngle_max = 70;//limitターゲット用(差分が視線の遊び)
        protected float _searchAngle_Head = 55;//胸ベース
        protected float _searchAngle_Eye = 40;//顔ベース

        [Header("＜パラメーター頭用＞")]
        public float inputWeight_Head = 0.0f;
        [SerializeField] protected float _leapVal_Head = 0;
        protected float _angle_head;

        [Header("＜パラメーター目用＞")]
        public float inputWeight_Eye = 0.0f;
        [SerializeField] protected float _leapVal_Eye = 0;
        protected float _angle_eye;

        [Tooltip("目の感度係数")]
        [SerializeField] protected Vector2 _eye_Amplitude;
        [Tooltip("最終的な注視の値")]
        [SerializeField] protected Vector3 _result_EyeLook;

        protected virtual void Awake()
        {
            _charaCon = GetComponent<CharaController>();
            _animator = GetComponent<Animator>();
            test = GetComponent<NormalizedBoneGenerator>();
            _lookTarget = GameObject.FindGameObjectWithTag("MainCamera").gameObject.transform;
        }

        protected void HeadUpdateBase()
        {
            //入力があるか
            if (0.0f < inputWeight_Head)
            {
                //胸ベース
                _angle_head = Vector3.Angle(test.virtualChest.forward.GetHorizontalDirection(), (_lookTarget.position - test.virtualChest.position).GetHorizontalDirection());
                //頭の限界用、角度を維持できないので仕方ない..
                test.lookTarget_limit.position = Vector3.Lerp(test.virtualChest.position + test.virtualChest.forward, _lookTarget.position, _searchAngle_Head / _angle_head);

                if (_searchAngle_max > _angle_head) _leapVal_Head += Time.deltaTime;
                else _leapVal_Head -= Time.deltaTime;
                _leapVal_Head = Mathf.Clamp(_leapVal_Head, 0.0f, inputWeight_Head);
            }
            else _leapVal_Head = 0;//初期化
        }

        protected void EyeUpdateBase()
        {
            if (0.0f < inputWeight_Eye)
            {
                //顔ベース
                _angle_eye = Vector3.Angle(test.virtualHead.forward.GetHorizontalDirection(), (_lookTarget.position - test.virtualChest.position).GetHorizontalDirection());

                if (_searchAngle_Eye > _angle_eye) _leapVal_Eye += Time.deltaTime * 2.0f;
                else _leapVal_Eye -= Time.deltaTime * 2.0f;
                _leapVal_Eye = Mathf.Clamp(_leapVal_Eye, 0.0f, inputWeight_Eye);
            }
            else _leapVal_Eye = 0;//初期化
        }
    }
}