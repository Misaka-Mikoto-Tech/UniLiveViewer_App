using UnityEngine;
//using UnityEngine.Animations.Rigging;
using VRM;

namespace UniLiveViewer 
{
    public class LookAt_VRMBlendShape : LookAtBase, IHeadLookAt, IEyeLookAt, ILookAtVRM
    {
        [Header("＜LookAt(VRM用、自動)＞")]
        public VRMLookAtBlendShapeApplyer_Custom _vrmEyeApplyer;


        protected override void Awake()
        {
            base.Awake();

            // TODO: 実行順見直す
            //チェック
            //if (_charaCon.charaInfoData.charaType != CharaInfoData.CHARATYPE.VRM_BlendShape)
            //{
            //    Debug.LogError("LookAtタイプが異なります");
            //    this.enabled = false;
            //}
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

            _result_EyeLook.x = 90;
            _result_EyeLook.y = _leapVal_Eye * 90;

            //目にオフセットを反映
            if (_vrmEyeApplyer)
            {
                _vrmEyeApplyer.Horizontal.CurveXRangeDegree = _result_EyeLook.x;
                _vrmEyeApplyer.Horizontal.CurveYRangeDegree = _result_EyeLook.y;

                _vrmEyeApplyer.VerticalDown.CurveXRangeDegree = _result_EyeLook.x;
                _vrmEyeApplyer.VerticalDown.CurveYRangeDegree = _result_EyeLook.y;
                _vrmEyeApplyer.VerticalUp.CurveXRangeDegree = _result_EyeLook.x;
                _vrmEyeApplyer.VerticalUp.CurveYRangeDegree = _result_EyeLook.y;
            }
        }

        /// <summary>
        /// 注視の有効/無効
        /// </summary>
        public void SetEnable(bool isEnable)
        {
            _vrmEyeApplyer.enabled = isEnable;
        }

        /// <summary>
        /// 注視リセット
        /// </summary>
        public void EyeReset()
        {
            _vrmEyeApplyer.Horizontal.CurveXRangeDegree = 0;
            _vrmEyeApplyer.Horizontal.CurveYRangeDegree = 0;

            _vrmEyeApplyer.VerticalDown.CurveXRangeDegree = 0;
            _vrmEyeApplyer.VerticalDown.CurveYRangeDegree = 0;
            _vrmEyeApplyer.VerticalUp.CurveXRangeDegree = 0;
            _vrmEyeApplyer.VerticalUp.CurveYRangeDegree = 0;
        }

        /// <summary>
        /// 注視ターゲット
        /// </summary>
        public Transform GetLookAtTarget()
        {
            return _lookTarget;
        }
    }
}