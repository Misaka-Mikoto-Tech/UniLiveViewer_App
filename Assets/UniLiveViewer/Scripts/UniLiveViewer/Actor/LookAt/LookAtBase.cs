using NanaCiel;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// AnimationControllerの管理はAnimationService側で完結している
    /// </summary>
    [RequireComponent(typeof(Animator))]

    public abstract class LookAtBase : MonoBehaviour
    {
        /// <summary>
        /// limitターゲット用(差分が視線の遊び)
        /// </summary>
        const float MaxsearchAngle = 70;
        /// <summary>
        /// 胸ベース
        /// </summary>
        const float SearchAngle_Head = 55;
        /// <summary>
        /// 顔ベース
        /// </summary>
        const float SearchAngle_Eye = 40;

        [Header("--- 設定 ---")]
        [Tooltip("目の感度係数")]
        [SerializeField] protected Vector2 _eye_Amplitude;
        [Tooltip("最終的な注視の値")]
        [SerializeField] protected Vector3 _result_EyeLook;

        [Header("--- 確認 ---")]
        [SerializeField] float _headInputWeight = 0.0f;
        [SerializeField] protected float _headLeap = 0.0f;
        protected float _headAngle;

        [SerializeField] float _eyeInputWeight = 0.0f;
        [SerializeField] protected float _eyeLeap = 0.0f;
        protected float _eyeAngle;

        public NormalizedBoneGenerator Test => _test;
        protected NormalizedBoneGenerator _test;

        public Transform LookTarget => _lookTarget;
        protected Transform _lookTarget;
        protected Animator _animator;

        protected bool _isHeadLookAt = false;
        protected　bool _isEyeLookAt = true;

        protected CharaInfoData _charaInfoData;

        protected virtual void Awake()
        {
            _test = transform.gameObject.AddComponent<NormalizedBoneGenerator>();
        }

        public virtual void Setup(Animator animator,CharaInfoData charaInfoData, Transform lookTarget)
        {
            _animator = animator;
            _charaInfoData = charaInfoData;
            _lookTarget = lookTarget;
        }

        public void SetHeadWeight(float w)
        {
            _headInputWeight = w;
        }
        public void SetEyeWeight(float w)
        {
            _eyeInputWeight = w;
        }

        protected void UpdateBaseHead()
        {
            if (0.0f < _headInputWeight)
            {
                //胸ベース
                _headAngle = Vector3.Angle(_test.virtualChest.forward.GetHorizontalDirection(),
                    (_lookTarget.position - _test.virtualChest.position).GetHorizontalDirection());
                //頭の限界用、角度を維持できないので仕方ない..
                _test.lookTarget_limit.position = Vector3.Lerp(_test.virtualChest.position + _test.virtualChest.forward, _lookTarget.position, SearchAngle_Head / _headAngle);

                if (MaxsearchAngle > _headAngle) _headLeap += Time.deltaTime;
                else _headLeap -= Time.deltaTime;
                _headLeap = Mathf.Clamp(_headLeap, 0.0f, _headInputWeight);
            }
            else _headLeap = 0;//初期化
        }

        protected void UpdateBaseEye()
        {
            if (0.0f < _eyeInputWeight)
            {
                //顔ベース
                _eyeAngle = Vector3.Angle(_test.virtualHead.forward.GetHorizontalDirection(),
                    (_lookTarget.position - _test.virtualChest.position).GetHorizontalDirection());

                if (SearchAngle_Eye > _eyeAngle) _eyeLeap += Time.deltaTime * 2.0f;
                else _eyeLeap -= Time.deltaTime * 2.0f;
                _eyeLeap = Mathf.Clamp(_eyeLeap, 0.0f, _eyeInputWeight);
            }
            else _eyeLeap = 0;//初期化
        }
    }
}