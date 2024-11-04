using NanaCiel;
using UnityEngine;
using UniVRM10;
using static UniVRM10.VRM10ObjectLookAt;

namespace UniLiveViewer.Actor.LookAt.VRM
{
    public class VRM10EyeLookAt : IEyeLookAt
    {
        /// <summary> 顔ベース </summary>
        const float SearchAngle_Eye = 40;
        /// <summary> 標準だと物足りない人用に調整幅持たせる </summary>
        const float Magnification = 1.5f;

        Vrm10Instance _instance;
        float _inputWeight = 0.0f;
        Transform _lookTarget;
        /// <summary>
        /// 更新が走ってエラるのでSetup完了までは無効化
        /// </summary>
        bool _isLookAt = false;
        float _eyeLeap = 0.0f;
        readonly CharaInfoData _charaInfoData;
        readonly NormalizedBoneGenerator _test;

        public VRM10EyeLookAt(
            CharaInfoData charaInfoData,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _charaInfoData = charaInfoData;
            _test = normalizedBoneGenerator;
        }

        public void Setup(Transform lookTarget, Vrm10Instance vrm10Instance)
        {
            _lookTarget = lookTarget;
            //vrm10Instance.LookAtTargetType = LookAtTargetTypes.SpecifiedTransform;//調整できないモード
            vrm10Instance.LookAtTargetType = LookAtTargetTypes.YawPitchValue;
            vrm10Instance.LookAtTarget = lookTarget;
            _instance = vrm10Instance;
            _charaInfoData.ExpressionType = ExpressionType.VRM10;
            _isLookAt = true;
        }

        void IEyeLookAt.SetEnable(bool isEnable)
        {
            _isLookAt = isEnable;
        }

        void IEyeLookAt.SetWeight(float weight)
        {
            _inputWeight = weight;
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_isLookAt == false) return;
            UpdateBaseEye();

            var v = _test.VirtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            var yaw = v.x * 180;
            var pitch = v.y * 90;
            _instance.Runtime.LookAt.SetYawPitchManually(yaw * _eyeLeap * Magnification, pitch * _eyeLeap * Magnification);
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
            else
            {
                _eyeLeap = 0;//初期化
            }
        }

        void IEyeLookAt.Reset()
        {
            _instance.Runtime.LookAt.SetYawPitchManually(0, 0);
        }
    }
}