using NanaCiel;
using UnityEngine;

namespace UniLiveViewer.Actor.LookAt.FBX
{
    /// <summary>
    /// VketChan / VketChanFurisode
    /// </summary>
    public class FBXEyeLookAtBlendShape : IEyeLookAt
    {
        /// <summary>
        /// 顔ベース
        /// </summary>
        const float SearchAngle_Eye = 40;

        float _inputWeight = 0.0f;
        Transform _lookTarget;
        /// <summary>
        /// 更新が走ってエラるのでSetup完了までは無効化
        /// </summary>
        bool _isLookAt = false;
        float _eyeLeap = 0.0f;

        readonly LookAtSettings _settings;
        readonly CharaInfoData _charaInfoData;
        readonly NormalizedBoneGenerator _test;

        public FBXEyeLookAtBlendShape(
            LookAtSettings settings,
            CharaInfoData charaInfoData,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _settings = settings;
            _charaInfoData = charaInfoData;
            _test = normalizedBoneGenerator;
        }

        public void Setup(Transform lookTarget)
        {
            _lookTarget = lookTarget;
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

            //var v = _test.virtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            switch (_charaInfoData.ExpressionType)
            {
                case ExpressionType.VketChan:
                    var result = _settings.eyeAmplitude.x * _eyeLeap;
                    _settings.Face.SetBlendShapeWeight(_settings.LookAtBlendShapeIndex, result);
                    break;
            }
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
            // TODO
        }
    }
}