using UnityEngine;
using UniVRM10;
using static UniVRM10.VRM10ObjectLookAt;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// 現状目は常時ONになってしまう
    /// TODO: LookAtBaseとLookatServiceから改修し、補間対応もする
    /// </summary>
    public class LookAt_VRM10 : LookAtBase, IHeadLookAt, IEyeLookAt
    {
        [SerializeField] Vrm10Instance _instance;

        public void Setup(Vrm10Instance vrm10Instance)
        {
            _instance = vrm10Instance;
            _instance.LookAtTargetType = LookAtTargetTypes.SpecifiedTransform;
            _instance.LookAtTarget = Camera.main.transform;//バラバラだ
        }

        void IHeadLookAt.SetEnable(bool isEnable)
        {
            _isHeadLookAt = isEnable;
        }

        void IHeadLookAt.OnLateTick()
        {
            if (_animator == null || _isHeadLookAt == false) return;
            UpdateBaseHead();
        }

        void OnAnimatorIK()
        {
            if (_animator == null || _isHeadLookAt == false) return;
            //全体、体、頭、目
            _animator.SetLookAtWeight(1.0f, 0.0f, _headLeap, 0.0f);
            _animator.SetLookAtPosition(_test.lookTarget_limit.position);
        }

        void IEyeLookAt.SetEnable(bool isEnable)
        {
            _isEyeLookAt = isEnable;

            if (isEnable)
            {
                _instance.LookAtTargetType = LookAtTargetTypes.SpecifiedTransform;
            }
            else
            {
                _instance.LookAtTargetType = LookAtTargetTypes.YawPitchValue;
            }
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_animator == null || _isEyeLookAt == false) return;

            UpdateBaseEye();
        }

        void IEyeLookAt.Reset()
        {
            _instance.Runtime.LookAt.SetYawPitchManually(0, 0);
        }
    }
}