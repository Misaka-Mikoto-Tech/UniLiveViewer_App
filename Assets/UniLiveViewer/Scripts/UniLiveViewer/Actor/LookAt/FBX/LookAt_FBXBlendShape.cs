using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// VketChan / VketChanFurisode
    /// </summary>
    public class LookAt_FBXBlendShape : LookAtBase, IHeadLookAt, IEyeLookAt
    {
        [Header("＜LookAt(プリセットキャラ用)＞")]
        [SerializeField] SkinnedMeshRenderer _skinMesh_Face;

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
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_animator == null || _isEyeLookAt == false) return;

            UpdateBaseEye();

            var v = _test.virtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            switch (_charaInfoData.ExpressionType)
            {
                case ExpressionType.VketChan:
                    _result_EyeLook.x = _eye_Amplitude.x * _eyeLeap;
                    _skinMesh_Face.SetBlendShapeWeight(14, _result_EyeLook.x);
                    break;
            }
        }

        void IEyeLookAt.Reset()
        {
            // TODO
        }
    }
}