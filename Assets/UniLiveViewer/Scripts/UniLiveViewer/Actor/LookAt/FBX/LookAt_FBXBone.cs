using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// UnityChanSSU / UnityChanKAGURA
    /// </summary>
    public class LookAt_FBXBone : LookAtBase, IHeadLookAt, IEyeLookAt
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
            _animator.SetLookAtWeight(1.0f, 0.0f, _headLeap, _result_EyeLook.x);
            _animator.SetLookAtPosition(_lookTarget.position);
        }

        void IEyeLookAt.SetEnable(bool isEnable)
        {
            _isEyeLookAt = isEnable;
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_animator == null || _isEyeLookAt == false) return;

            UpdateBaseEye();

            switch (_charaInfoData.ExpressionType)
            {
                case ExpressionType.UnityChanSSU:
                case ExpressionType.UnityChanKAGURA:
                    _result_EyeLook.x = _eye_Amplitude.x * _eyeLeap;
                    break;
            }
        }

        void IEyeLookAt.Reset()
        {
            // TODO
        }
    }
}