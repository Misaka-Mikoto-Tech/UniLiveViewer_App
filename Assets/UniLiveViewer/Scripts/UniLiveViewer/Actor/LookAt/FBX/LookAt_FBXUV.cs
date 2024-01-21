using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// UnityChan / CandyRockStar / UnityChanSD
    /// </summary>
    public class LookAt_FBXUV : LookAtBase, IHeadLookAt, IEyeLookAt
    {
        [Header("＜LookAt(プリセットキャラ用)＞")]
        [SerializeField] SkinnedMeshRenderer _skinMesh_Face;

        //手動で開放するため
        Material _eyeMat;

        public override void Setup(Animator animator, CharaInfoData charaInfoData, Transform lookTarget)
        {
            base.Setup(animator, charaInfoData, lookTarget);
            if (_charaInfoData.ExpressionType == ExpressionType.UnityChan
                || _charaInfoData.ExpressionType == ExpressionType.CandyChan)
            {
                _eyeMat = _skinMesh_Face.material;
            }
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
        }

        void IEyeLookAt.OnLateTick()
        {
            if (_animator == null || _isEyeLookAt == false) return;

            UpdateBaseEye();

            var v = _test.virtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            switch (_charaInfoData.ExpressionType)
            {
                case ExpressionType.UnityChan:
                case ExpressionType.CandyChan:
                    //ローカル座標に変換
                    _result_EyeLook.x = v.x * _eye_Amplitude.x * _eyeLeap;
                    _result_EyeLook.y = -v.y * _eye_Amplitude.y * _eyeLeap;
                    //UVをオフセットを反映
                    _eyeMat.SetTextureOffset("_BaseMap", _result_EyeLook);
                    break;
                case ExpressionType.UnityChanSD:
                    //ローカル座標に変換
                    _result_EyeLook.x = -v.y * _eye_Amplitude.x * _eyeLeap;
                    _result_EyeLook.y = v.x * _eye_Amplitude.y * _eyeLeap;

                    _test.lEyeAnchor.localRotation = Quaternion.Euler(new Vector3(_result_EyeLook.x, 0, _result_EyeLook.y));
                    _test.rEyeAnchor.localRotation = Quaternion.Euler(new Vector3(_result_EyeLook.x, 0, _result_EyeLook.y));
                    break;
            }
        }

        void IEyeLookAt.Reset()
        {
            // TODO
        }

        void OnDestroy()
        {
            if (_eyeMat) Destroy(_eyeMat);
        }
    }
}