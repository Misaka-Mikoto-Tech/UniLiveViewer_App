using NanaCiel;
using UnityEngine;
using UniVRM10;
using static UniVRM10.VRM10ObjectLookAt;

namespace UniLiveViewer.Actor.LookAt.VRM
{
    /// <summary>
    /// TODO: 現状目は常時ONになってしまう
    /// </summary>
    public class VRM10EyeLookAt : IEyeLookAt
    {
        /// <summary>
        /// 顔ベース
        /// </summary>
        const float SearchAngle_Eye = 40;

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
            vrm10Instance.LookAtTargetType = LookAtTargetTypes.SpecifiedTransform;
            vrm10Instance.LookAtTarget = lookTarget;
            _instance = vrm10Instance;
            _charaInfoData.ExpressionType = ExpressionType.VRM10;
            _isLookAt = true;
        }

        void IEyeLookAt.SetEnable(bool isEnable)
        {
            _isLookAt = isEnable;

            if (isEnable)
            {
                _instance.LookAtTargetType = LookAtTargetTypes.SpecifiedTransform;
            }
            else
            {
                _instance.LookAtTargetType = LookAtTargetTypes.YawPitchValue;
            }
        }

        void IEyeLookAt.SetWeight(float weight)
        {
            _inputWeight = weight;
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_isLookAt == false) return;

            //UpdateBaseEye();
        }

        /// <summary>
        /// TODO: 補間ロジックはLookAt側ではなくtarget側にしようかな...YawPitchValueでやるのはめんど
        /// >NormalizedBoneGenerator？
        /// </summary>
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
            _instance.Runtime.LookAt.SetYawPitchManually(0, 0);
        }
    }
}