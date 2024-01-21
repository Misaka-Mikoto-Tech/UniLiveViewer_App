using UnityEngine;
using VRM;

namespace UniLiveViewer.Actor.LookAt
{
    public class LookAt_VRMBlendShape : LookAtBase, IHeadLookAt, IEyeLookAt
    {
        [SerializeField] VRMLookAtBlendShapeApplyer _applyer;

        public void Setup(VRMLookAtBlendShapeApplyer applyer)
        {
            _applyer = applyer;
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
            _applyer.enabled = isEnable;
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_animator == null || _isEyeLookAt == false) return;

            UpdateBaseEye();

            _result_EyeLook.x = 90;
            _result_EyeLook.y = _eyeLeap * 90;

            //目にオフセットを反映
            if (_applyer)
            {
                _applyer.Horizontal.CurveXRangeDegree = _result_EyeLook.x;
                _applyer.Horizontal.CurveYRangeDegree = _result_EyeLook.y;

                _applyer.VerticalDown.CurveXRangeDegree = _result_EyeLook.x;
                _applyer.VerticalDown.CurveYRangeDegree = _result_EyeLook.y;
                _applyer.VerticalUp.CurveXRangeDegree = _result_EyeLook.x;
                _applyer.VerticalUp.CurveYRangeDegree = _result_EyeLook.y;
            }
        }

        void IEyeLookAt.Reset()
        {
            _applyer.Horizontal.CurveXRangeDegree = 0;
            _applyer.Horizontal.CurveYRangeDegree = 0;

            _applyer.VerticalDown.CurveXRangeDegree = 0;
            _applyer.VerticalDown.CurveYRangeDegree = 0;
            _applyer.VerticalUp.CurveXRangeDegree = 0;
            _applyer.VerticalUp.CurveYRangeDegree = 0;
        }
    }
}