using UnityEngine;
//using UnityEngine.Animations.Rigging;

namespace UniLiveViewer 
{
    //UnityChan / CandyRockStar / UnityChanSD
    public class LookAt_FBXUV : LookAtBase, IHeadLookAt, IEyeLookAt
    {
        [Header("＜LookAt(プリセットキャラ用)＞")]
        [SerializeField] SkinnedMeshRenderer _skinMesh_Face;

        //手動で開放するため
        Material _eyeMat;

        protected override void Awake()
        {
            base.Awake();

            if (_charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChan
                || _charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.CandyChan)
            {
                _eyeMat = _skinMesh_Face.material;
            }
        }

        /// <summary>
        /// 頭の注視処理
        /// </summary>
        public void HeadUpdate()
        {
            HeadUpdateBase();
        }

        public void HeadUpdate_OnAnimatorIK()
        {
            //全体、体、頭、目
            _animator.SetLookAtWeight(1.0f, 0.0f, _leapVal_Head, 0.0f);
            _animator.SetLookAtPosition(test.lookTarget_limit.position);
        }

        /// <summary>
        /// 目の注視処理
        /// </summary>
        public void EyeUpdate()
        {
            EyeUpdateBase();

            Vector3 v = test.virtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            switch (_charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.UnityChan:
                case CharaInfoData.CHARATYPE.CandyChan:
                    //ローカル座標に変換
                    _result_EyeLook.x = v.x * _eye_Amplitude.x * _leapVal_Eye;
                    _result_EyeLook.y = -v.y * _eye_Amplitude.y * _leapVal_Eye;
                    //UVをオフセットを反映
                    _eyeMat.SetTextureOffset("_BaseMap", _result_EyeLook);
                    break;
                case CharaInfoData.CHARATYPE.UnityChanSD:
                    //ローカル座標に変換
                    _result_EyeLook.x = -v.y * _eye_Amplitude.x * _leapVal_Eye;
                    _result_EyeLook.y = v.x * _eye_Amplitude.y * _leapVal_Eye;

                    test.lEyeAnchor.localRotation = Quaternion.Euler(new Vector3(_result_EyeLook.x, 0, _result_EyeLook.y));
                    test.rEyeAnchor.localRotation = Quaternion.Euler(new Vector3(_result_EyeLook.x, 0, _result_EyeLook.y));
                    break;
            }
        }

        void OnDestroy()
        {
            if (_eyeMat != null) Destroy(_eyeMat);
        }
    }
}