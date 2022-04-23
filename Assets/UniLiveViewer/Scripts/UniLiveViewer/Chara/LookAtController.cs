using UnityEngine;
using VRM;
//using UnityEngine.Animations.Rigging;
using NanaCiel;

namespace UniLiveViewer 
{
    //改善の余地しかない
    public class LookAtController : MonoBehaviour
    {
        [Header("＜LookAt(プリセットキャラ用)＞")]
        [SerializeField] private SkinnedMeshRenderer skinMesh_Face;

        [Header("＜LookAt(VRM用、自動)＞")]
        public VRMLookAtBoneApplyer_Custom VRMLookAtEye_Bone = null;
        public VRMLookAtBlendShapeApplyer_Custom VRMLookAtEye_UV = null;

        [Header("＜共有(自動管理)＞")]
        public Transform virtualEye;//正面用
        public Transform virtualHead;//正面用
        public Transform virtualRoot;//正面用

        //各種ボーンAnchorを取得
        [HideInInspector] public Transform hipAnchor;
        [HideInInspector] public Transform headAnchor;
        [HideInInspector] public Transform chestAnchor;
        [HideInInspector] public Transform lEyeAnchor;
        [HideInInspector] public Transform rEyeAnchor;

        public Transform lookTarget;
        private Animator animator;
        private CharaController charaCon;
        //private HeadRigController headRigCon;

        [Header("＜パラメーター頭用＞")]
        public float inputWeight_Head = 0.0f;
        [SerializeField] private float searchAngle_Head = 60;
        [SerializeField] private float leapVal_Head = 0;
        private float leapSpeed_head = 0;
        private float angle_head;

        [Header("＜パラメーター目用＞")]
        public float inputWeight_Eye = 0.0f;
        [SerializeField] private float searchAngle_Eye = 70;
        [SerializeField] private float leapVal_Eye = 0;
        private float leapSpeed_eye = 0;
        private float angle_eye;

        [Tooltip("目の感度係数"), SerializeField] private Vector2 eye_Amplitude;
        [Tooltip("最終的な注視の値"), SerializeField] private Vector3 result_EyeLook;

        //旧Unityちゃん用(手動で開放するため)
        private Material eyeMat;

        void Awake()
        {
            if (!charaCon) charaCon = GetComponent<CharaController>();
            if (!animator) animator = GetComponent<Animator>();
            if (!lookTarget) lookTarget = GameObject.FindGameObjectWithTag("MainCamera").gameObject.transform;


            //各種ボーンからアンカーを取得
            hipAnchor = animator.GetBoneTransform(HumanBodyBones.Hips);
            headAnchor = animator.GetBoneTransform(HumanBodyBones.Head);
            chestAnchor = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (!chestAnchor) chestAnchor = animator.GetBoneTransform(HumanBodyBones.Chest);
            lEyeAnchor = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            rEyeAnchor = animator.GetBoneTransform(HumanBodyBones.RightEye);

            if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChan
                || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.CandyChan)
            {
                eyeMat = skinMesh_Face.material;
            }
        }

        /// <summary>
        /// VRM用のコンポーネント参照設定
        /// </summary>
        /// <param name="anime"></param>
        /// <param name="charaController"></param>
        /// <param name="lookAtTarget"></param>
        public void SetVRMComponent(Animator anime, CharaController charaController, Transform lookAtTarget)
        {
            animator = anime;
            charaCon = charaController;
            charaCon.lookAtCon = this;
            lookTarget = lookAtTarget;
        }


        // Start is called before the first frame update
        void Start()
        {
            if (virtualRoot) return;//Prefab対策

            //仮想ルートを生成(頭の正面用)
            virtualRoot = new GameObject("VirtualRoot").transform;
            virtualRoot.parent = hipAnchor;
            virtualRoot.gameObject.layer = SystemInfo.layerNo_VirtualHead;
            //virtualRoot.gameObject.layer = Parameters.layer_VirtualHead;
            virtualRoot.localPosition = Vector3.zero;
            virtualRoot.rotation = transform.rotation;

            //仮想ヘッドを生成(目の正面用)
            virtualHead = new GameObject("VirtualHead").transform;
            virtualHead.parent = headAnchor.parent;
            virtualHead.gameObject.layer = SystemInfo.layerNo_VirtualHead;
            virtualHead.gameObject.AddComponent(typeof(SphereCollider));
            var col = virtualHead.GetComponent<SphereCollider>();
            col.radius = 0.06f;
            col.isTrigger = true;
            virtualHead.localPosition = Vector3.zero;
            virtualHead.rotation = transform.rotation;

            //仮想アイを生成(いる？)
            virtualEye = new GameObject("VirtualEye").transform;
            virtualEye.parent = headAnchor;
            virtualEye.gameObject.layer = SystemInfo.layerNo_VirtualHead;
            virtualEye.localPosition = Vector3.zero;
            virtualEye.rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            //ポーズ中なら以下処理しない
            if (Time.timeScale == 0) return;

            LookAt_Head();

            LookAt_Eye();
        }

        /// <summary>
        /// 頭の注視処理
        /// </summary>
        private void LookAt_Head()
        {
            //入力があるか
            if (0.0f < inputWeight_Head)
            {
                //水平上のターゲットとの角度を取得
                angle_head = GetHorizontalAngle(lookTarget, virtualRoot);

                //正面に近いほど速度を上げる
                leapSpeed_head = Time.deltaTime * Mathf.Clamp((searchAngle_Head / angle_head), 0, 1);

                //視覚内なら徐々に1へ(対象の方を向く)
                if (searchAngle_Head > angle_head)
                {
                    leapVal_Head = Mathf.Clamp(leapVal_Head + leapSpeed_head, 0.0f, inputWeight_Head);
                }
                //視覚外なら徐々に0へ(正面を向く)
                else
                {
                    leapVal_Head = Mathf.Clamp(leapVal_Head - leapSpeed_head, 0.0f, inputWeight_Head);
                }
            }
            else
            {
                leapVal_Head = 0;//初期化
            }
        }

        /// <summary>
        /// 目の注視処理
        /// </summary>
        private void LookAt_Eye()
        {
            //入力があるか
            if (0.0f < inputWeight_Eye)
            {
                //水平上のターゲットとの角度を取得
                angle_eye = GetHorizontalAngle(lookTarget, virtualHead);

                //正面に近いほど速度を上げる
                leapSpeed_eye = Time.deltaTime * Mathf.Clamp((searchAngle_Eye / angle_eye), 0, 1);

                //視覚内なら徐々に1へ(対象の方を向く)
                if (searchAngle_Head > angle_head)
                {
                    leapVal_Eye = Mathf.Clamp(leapVal_Eye + leapSpeed_eye, 0.0f, inputWeight_Eye);
                }
                //視覚外なら徐々に0へ(正面を向く)
                else
                {
                    leapVal_Eye = Mathf.Clamp(leapVal_Eye - leapSpeed_eye, 0.0f, inputWeight_Eye);
                }
            }
            else
            {
                leapVal_Eye = 0;//初期化
            }

            //銀の弾ないの？
            Vector3 v;
            switch (charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.UnityChan:
                    //ローカル座標に変換
                    v = virtualEye.InverseTransformPoint(lookTarget.position).normalized;
                    result_EyeLook.x = v.x * eye_Amplitude.x * leapVal_Eye;
                    result_EyeLook.y = -v.y * eye_Amplitude.y * leapVal_Eye;
                    //UVをオフセットを反映
                    eyeMat.SetTextureOffset("_BaseMap", result_EyeLook);
                    break;
                case CharaInfoData.CHARATYPE.CandyChan:
                    //ローカル座標に変換
                    v = virtualEye.InverseTransformPoint(lookTarget.position).normalized;
                    result_EyeLook.x = v.x * eye_Amplitude.x * leapVal_Eye;
                    result_EyeLook.y = -v.y * eye_Amplitude.y * leapVal_Eye;
                    //UVをオフセットを反映
                    eyeMat.SetTextureOffset("_BaseMap", result_EyeLook);
                    break;
                case CharaInfoData.CHARATYPE.UnityChanSSU:
                    result_EyeLook.x = eye_Amplitude.x * leapVal_Eye;
                    break;
                case CharaInfoData.CHARATYPE.UnityChanSD:
                    //ローカル座標に変換
                    v = virtualEye.InverseTransformPoint(lookTarget.position).normalized;
                    result_EyeLook.x = -v.y * eye_Amplitude.x * leapVal_Eye;
                    result_EyeLook.y = v.x * eye_Amplitude.y * leapVal_Eye;
                    lEyeAnchor.localRotation = Quaternion.Euler(new Vector3(result_EyeLook.x, 0, result_EyeLook.y));
                    rEyeAnchor.localRotation = Quaternion.Euler(new Vector3(result_EyeLook.x, 0, result_EyeLook.y));
                    break;
                case CharaInfoData.CHARATYPE.VketChan:
                    result_EyeLook.x = eye_Amplitude.x * leapVal_Eye;
                    skinMesh_Face.SetBlendShapeWeight(14, result_EyeLook.x);
                    break;
                case CharaInfoData.CHARATYPE.UnityChanKAGURA:
                    result_EyeLook.x = eye_Amplitude.x * leapVal_Eye;
                    break;
                case CharaInfoData.CHARATYPE.VRM_Bone:
                    result_EyeLook.x = 90;
                    result_EyeLook.y = leapVal_Eye * 90;
                    //目にオフセットを反映
                    if (VRMLookAtEye_Bone)
                    {
                        VRMLookAtEye_Bone.HorizontalOuter.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_Bone.HorizontalOuter.CurveYRangeDegree = result_EyeLook.y;
                        VRMLookAtEye_Bone.HorizontalInner.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_Bone.HorizontalInner.CurveYRangeDegree = result_EyeLook.y;

                        VRMLookAtEye_Bone.VerticalDown.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_Bone.VerticalDown.CurveYRangeDegree = result_EyeLook.y;
                        VRMLookAtEye_Bone.VerticalUp.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_Bone.VerticalUp.CurveYRangeDegree = result_EyeLook.y;
                    }
                    break;
                case CharaInfoData.CHARATYPE.VRM_BlendShape:
                    result_EyeLook.x = 90;
                    result_EyeLook.y = leapVal_Eye * 90;
                    //目にオフセットを反映
                    if (VRMLookAtEye_UV)
                    {
                        VRMLookAtEye_UV.Horizontal.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_UV.Horizontal.CurveYRangeDegree = result_EyeLook.y;

                        VRMLookAtEye_UV.VerticalDown.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_UV.VerticalDown.CurveYRangeDegree = result_EyeLook.y;
                        VRMLookAtEye_UV.VerticalUp.CurveXRangeDegree = result_EyeLook.x;
                        VRMLookAtEye_UV.VerticalUp.CurveYRangeDegree = result_EyeLook.y;
                    }
                    break;
            }

        }

        public void SetEnable_VRMLookAtEye(bool isEnable)
        {
            if (VRMLookAtEye_Bone) VRMLookAtEye_Bone.enabled = isEnable;
            else if (VRMLookAtEye_UV) VRMLookAtEye_UV.enabled = isEnable;
        }

        public void Reset_VRMLookAtEye()
        {
            if (VRMLookAtEye_Bone)
            {
                VRMLookAtEye_Bone.HorizontalOuter.CurveXRangeDegree = 0;
                VRMLookAtEye_Bone.HorizontalOuter.CurveYRangeDegree = 0;
                VRMLookAtEye_Bone.HorizontalInner.CurveXRangeDegree = 0;
                VRMLookAtEye_Bone.HorizontalInner.CurveYRangeDegree = 0;

                VRMLookAtEye_Bone.VerticalDown.CurveXRangeDegree = 0;
                VRMLookAtEye_Bone.VerticalDown.CurveYRangeDegree = 0;
                VRMLookAtEye_Bone.VerticalUp.CurveXRangeDegree = 0;
                VRMLookAtEye_Bone.VerticalUp.CurveYRangeDegree = 0;
            }
            else if (VRMLookAtEye_UV)
            {
                VRMLookAtEye_UV.Horizontal.CurveXRangeDegree = 0;
                VRMLookAtEye_UV.Horizontal.CurveYRangeDegree = 0;

                VRMLookAtEye_UV.VerticalDown.CurveXRangeDegree = 0;
                VRMLookAtEye_UV.VerticalDown.CurveYRangeDegree = 0;
                VRMLookAtEye_UV.VerticalUp.CurveXRangeDegree = 0;
                VRMLookAtEye_UV.VerticalUp.CurveYRangeDegree = 0;
            }
        }

        /// <summary>
        /// 水平上のターゲットとの角度を取得
        /// </summary>
        /// <param name="target"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        private float GetHorizontalAngle(Transform target, Transform origin)
        {
            //プレイヤーの方向(水平)
            var playerDirection = (target.position - origin.position).GetHorizontalDirection();
            //角度
            return Vector3.Angle(origin.forward.GetHorizontalDirection(), playerDirection);
        }

        private void OnAnimatorIK()
        {
            switch (charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.UnityChanSSU:
                    //全体、体、頭、目
                    animator.SetLookAtWeight(1.0f, 0.0f, leapVal_Head, result_EyeLook.x);
                    animator.SetLookAtPosition(lookTarget.position);
                    break;
                case CharaInfoData.CHARATYPE.UnityChanKAGURA:
                    //全体、体、頭、目
                    animator.SetLookAtWeight(1.0f, 0.0f, leapVal_Head, result_EyeLook.x);
                    animator.SetLookAtPosition(lookTarget.position);
                    break;
                default:
                    //全体、体、頭、目
                    animator.SetLookAtWeight(1.0f, 0.0f, leapVal_Head, 0.0f);
                    animator.SetLookAtPosition(lookTarget.position);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (eyeMat != null) Destroy(eyeMat);
        }
    }
}