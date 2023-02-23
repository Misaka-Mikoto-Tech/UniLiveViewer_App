using UnityEngine;
//using UnityEngine.Animations.Rigging;

namespace UniLiveViewer 
{
    //UnityChan / CandyRockStar / UnityChanSD
    public class LookAt_FBXBlendShape : LookAtBase, IHeadLookAt, IEyeLookAt
    {
        [Header("＜LookAt(プリセットキャラ用)＞")]
        [SerializeField] SkinnedMeshRenderer _skinMesh_Face;

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 頭の注視処理
        /// </summary>
        void IHeadLookAt.HeadUpdate()
        {
            HeadUpdateBase();
        }

        void IHeadLookAt.HeadUpdate_OnAnimatorIK()
        {
            //全体、体、頭、目
            _animator.SetLookAtWeight(1.0f, 0.0f, _leapVal_Head, 0.0f);
            _animator.SetLookAtPosition(test.lookTarget_limit.position);
        }

        /// <summary>
        /// 目の注視処理
        /// </summary>
        void IEyeLookAt.EyeUpdate()
        {
            EyeUpdateBase();

            Vector3 v = test.virtualEye.InverseTransformPoint(_lookTarget.position).normalized;
            switch (_charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.VketChan:
                    _result_EyeLook.x = _eye_Amplitude.x * _leapVal_Eye;
                    _skinMesh_Face.SetBlendShapeWeight(14, _result_EyeLook.x);
                    break;
            }
        }
    }
}