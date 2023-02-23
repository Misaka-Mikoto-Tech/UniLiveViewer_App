using UnityEngine;
//using UnityEngine.Animations.Rigging;

namespace UniLiveViewer 
{
    //UnityChanSSU / UnityChanKAGURA
    public class LookAt_FBXBone : LookAtBase, IHeadLookAt, IEyeLookAt
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
            _animator.SetLookAtWeight(1.0f, 0.0f, _leapVal_Head, _result_EyeLook.x);
            _animator.SetLookAtPosition(_lookTarget.position);
        }

        /// <summary>
        /// 目の注視処理
        /// </summary>
        void IEyeLookAt.EyeUpdate()
        {
            EyeUpdateBase();

            switch (_charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.UnityChanSSU:
                case CharaInfoData.CHARATYPE.UnityChanKAGURA:
                    _result_EyeLook.x = _eye_Amplitude.x * _leapVal_Eye;
                    break;
            }
        }
    }
}