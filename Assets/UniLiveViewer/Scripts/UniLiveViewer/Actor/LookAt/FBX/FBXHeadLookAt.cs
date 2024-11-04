using NanaCiel;
using UnityEngine;

namespace UniLiveViewer.Actor.LookAt.FBX
{
    public class FBXHeadLookAt : IHeadLookAt
    {
        /// <summary>
        /// limitターゲット用(差分が視線の遊び)
        /// </summary>
        const float MaxsearchAngle = 70;
        /// <summary>
        /// 胸ベース
        /// </summary>
        const float SearchAngle = 55;

        float _inputWeight = 0.0f;
        float _leap = 0.0f;
        Transform _lookTarget;

        readonly MonoBehaviourLookAt _monoBehaviourLookAt;
        readonly NormalizedBoneGenerator _test;

        /// <summary>
        /// 更新が走ってエラるのでSetup完了までは無効化
        /// </summary>
        bool _isLookAt = false;

        public FBXHeadLookAt(
            MonoBehaviourLookAt monoBehaviourLookAt,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _monoBehaviourLookAt = monoBehaviourLookAt;
            _test = normalizedBoneGenerator;
        }

        public void Setup(Transform lookTarget)
        {
            _lookTarget = lookTarget;
            _isLookAt = true;
        }

        void IHeadLookAt.SetEnable(bool isEnable)
        {
            _isLookAt = isEnable;

            if (!isEnable)
            {
                _leap = 0;
                _monoBehaviourLookAt.ChangeHeadWeight(_leap);
            }
        }

        void IHeadLookAt.SetWeight(float weight)
        {
            _inputWeight = weight;
        }

        void IHeadLookAt.OnLateTick()
        {
            if (_isLookAt == false) return;

            UpdateBaseHead();
        }

        void UpdateBaseHead()
        {
            if (0.0f < _inputWeight)
            {
                //胸ベース
                var headAngle = Vector3.Angle(_test.VirtualChest.forward.GetHorizontalDirection(),
                    (_lookTarget.position - _test.VirtualChest.position).GetHorizontalDirection());
                //頭の限界用、角度を維持できないので仕方ない..
                _test.LookTargetLimit.position = Vector3.Lerp(
                    _test.VirtualChest.position + _test.VirtualChest.forward,
                    _lookTarget.position,
                    SearchAngle / headAngle);

                if (MaxsearchAngle > headAngle) _leap += Time.deltaTime;
                else _leap -= Time.deltaTime;
                _leap = Mathf.Clamp(_leap, 0.0f, _inputWeight);
            }
            else _leap = 0;//初期化

            _monoBehaviourLookAt.ChangeHeadWeight(_leap);
        }
    }
}