using NanaCiel;
using UnityEngine;
using VContainer;
using VRM;

namespace UniLiveViewer.Actor.LookAt.VRM
{
    public class VRMEyeLookAtBone : IEyeLookAt
    {
        /// <summary>
        /// 顔ベース
        /// </summary>
        const float SearchAngle_Eye = 40;

        VRMLookAtBoneApplyer _applyer;

        float _inputWeight = 0.0f;
        Transform _lookTarget;
        /// <summary>
        /// 更新が走ってエラるのでSetup完了までは無効化
        /// </summary>
        bool _isLookAt = false;
        float _eyeLeap = 0.0f;
        readonly CharaInfoData _charaInfoData;
        readonly NormalizedBoneGenerator _test;

        [Inject]
        public VRMEyeLookAtBone(
            CharaInfoData charaInfoData,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _charaInfoData = charaInfoData;
            _test = normalizedBoneGenerator;
        }

        public void Setup(Transform lookTarget, VRMLookAtBoneApplyer applyer)
        {
            _lookTarget = lookTarget;
            _applyer = applyer;
            _charaInfoData.ExpressionType = ExpressionType.VRM_Bone;
            _isLookAt = true;
        }

        void IEyeLookAt.SetEnable(bool isEnable)
        {
            _isLookAt = isEnable;
            _applyer.enabled = isEnable;
        }

        void IEyeLookAt.SetWeight(float weight)
        {
            _inputWeight = weight;
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_isLookAt == false) return;

            UpdateBaseEye();

            var result = Vector3.zero;

            result.x = 90;
            result.y = _eyeLeap * 90;

            //目にオフセットを反映
            _applyer.HorizontalOuter.CurveXRangeDegree = result.x;
            _applyer.HorizontalOuter.CurveYRangeDegree = result.y;
            _applyer.HorizontalInner.CurveXRangeDegree = result.x;
            _applyer.HorizontalInner.CurveYRangeDegree = result.y;

            _applyer.VerticalDown.CurveXRangeDegree = result.x;
            _applyer.VerticalDown.CurveYRangeDegree = result.y;
            _applyer.VerticalUp.CurveXRangeDegree = result.x;
            _applyer.VerticalUp.CurveYRangeDegree = result.y;
        }

        void UpdateBaseEye()
        {
            if (0.0f < _inputWeight)
            {
                //顔ベース
                var eyeAngle = Vector3.Angle(_test.VirtualHead.forward.GetHorizontalDirection(),
                    (_lookTarget.position - _test.VirtualChest.position).GetHorizontalDirection());

                if (SearchAngle_Eye > eyeAngle) _eyeLeap += Time.deltaTime * 2.0f;
                else _eyeLeap -= Time.deltaTime * 2.0f;
                _eyeLeap = Mathf.Clamp(_eyeLeap, 0.0f, _inputWeight);
            }
            else _eyeLeap = 0;//初期化
        }

        void IEyeLookAt.Reset()
        {
            _applyer.HorizontalOuter.CurveXRangeDegree = 0;
            _applyer.HorizontalOuter.CurveYRangeDegree = 0;
            _applyer.HorizontalInner.CurveXRangeDegree = 0;
            _applyer.HorizontalInner.CurveYRangeDegree = 0;

            _applyer.VerticalDown.CurveXRangeDegree = 0;
            _applyer.VerticalDown.CurveYRangeDegree = 0;
            _applyer.VerticalUp.CurveXRangeDegree = 0;
            _applyer.VerticalUp.CurveYRangeDegree = 0;
        }
    }
}