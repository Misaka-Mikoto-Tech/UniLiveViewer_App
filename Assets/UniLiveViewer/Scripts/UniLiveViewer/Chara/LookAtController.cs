using UnityEngine;
using VRM;
using UnityEngine.Animations.Rigging;
using NanaCiel;

namespace UniLiveViewer 
{
    //���P�̗]�n�����Ȃ�
    public class LookAtController : MonoBehaviour
    {
        [Header("��LookAt(�v���Z�b�g�L�����p)��")]
        [SerializeField] private SkinnedMeshRenderer skinMesh_Face;

        [Header("��LookAt(VRM�p�A����)��")]
        public VRMLookAtBoneApplyer_Custom VRMLookAtEye_Bone = null;
        public VRMLookAtBlendShapeApplyer_Custom VRMLookAtEye_UV = null;

        [Header("�����L(�����Ǘ�)��")]
        public Transform virtualEye;//���ʗp
        public Transform virtualHead;//���ʗp
        public Transform virtualRoot;//���ʗp

        //�e��{�[��Anchor���擾
        [HideInInspector] public Transform hipAnchor;
        [HideInInspector] public Transform headAnchor;
        [HideInInspector] public Transform chestAnchor;
        [HideInInspector] public Transform lEyeAnchor;
        [HideInInspector] public Transform rEyeAnchor;

        public Transform lookTarget;
        private Animator animator;
        private CharaController charaCon;
        private HeadRigController headRigCon;

        [Header("���p�����[�^�[���p��")]
        public float inputWeight_Head = 0.0f;
        [SerializeField] private float searchAngle_Head = 60;
        [SerializeField] private float leapVal_Head = 0;
        private float leapSpeed_head = 0;
        private float angle_head;

        [Header("���p�����[�^�[�ڗp��")]
        public float inputWeight_Eye = 0.0f;
        [SerializeField] private float searchAngle_Eye = 70;
        [SerializeField] private float leapVal_Eye = 0;
        private float leapSpeed_eye = 0;
        private float angle_eye;

        [Tooltip("�ڂ̊��x�W��"), SerializeField] private Vector2 eye_Amplitude;
        [Tooltip("�ŏI�I�Ȓ����̒l"), SerializeField] private Vector3 result_EyeLook;

        //��Unity�����p(�蓮�ŊJ�����邽��)
        private Material eyeMat;

        void Awake()
        {
            if (!charaCon) charaCon = GetComponent<CharaController>();
            if (!animator) animator = GetComponent<Animator>();
            if (!lookTarget) lookTarget = GameObject.FindGameObjectWithTag("MainCamera").gameObject.transform;


            //�e��{�[������A���J�[���擾
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
        /// VRM�p�̃R���|�[�l���g�Q�Ɛݒ�
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
            //���z���[�g�𐶐�(���̐��ʗp)
            virtualRoot = new GameObject("VirtualRoot").transform;
            virtualRoot.parent = hipAnchor;
            virtualRoot.gameObject.layer = Parameters.layerNo_VirtualHead;
            //virtualRoot.gameObject.layer = Parameters.layer_VirtualHead;
            virtualRoot.localPosition = Vector3.zero;
            virtualRoot.rotation = transform.rotation;

            //���z�w�b�h�𐶐�(�ڂ̐��ʗp)
            virtualHead = new GameObject("VirtualHead").transform;
            virtualHead.parent = headAnchor.parent;
            virtualHead.gameObject.layer = Parameters.layerNo_VirtualHead;
            virtualHead.gameObject.AddComponent(typeof(SphereCollider));
            var col = virtualHead.GetComponent<SphereCollider>();
            col.radius = 0.06f;
            col.isTrigger = true;
            virtualHead.localPosition = Vector3.zero;
            virtualHead.rotation = transform.rotation;

            //���z�A�C�𐶐�(����H)
            virtualEye = new GameObject("VirtualEye").transform;
            virtualEye.parent = headAnchor;
            virtualEye.gameObject.layer = Parameters.layerNo_VirtualHead;
            virtualEye.localPosition = Vector3.zero;
            virtualEye.rotation = transform.rotation;
        }

        private void LateUpdate()
        {
            //�|�[�Y���Ȃ�ȉ��������Ȃ�
            if (Time.timeScale == 0) return;

            LookAt_Head();

            LookAt_Eye();
        }

        /// <summary>
        /// ���̒�������
        /// </summary>
        private void LookAt_Head()
        {
            //���͂����邩
            if (0.0f < inputWeight_Head)
            {
                //������̃^�[�Q�b�g�Ƃ̊p�x���擾
                angle_head = GetHorizontalAngle(lookTarget, virtualRoot);

                //���ʂɋ߂��قǑ��x���グ��
                leapSpeed_head = Time.deltaTime * Mathf.Clamp((searchAngle_Head / angle_head), 0, 1);

                //���o���Ȃ珙�X��1��(�Ώۂ̕�������)
                if (searchAngle_Head > angle_head)
                {
                    leapVal_Head = Mathf.Clamp(leapVal_Head + leapSpeed_head, 0.0f, inputWeight_Head);
                }
                //���o�O�Ȃ珙�X��0��(���ʂ�����)
                else
                {
                    leapVal_Head = Mathf.Clamp(leapVal_Head - leapSpeed_head, 0.0f, inputWeight_Head);
                }
            }
            else
            {
                leapVal_Head = 0;//������
            }
        }

        /// <summary>
        /// �ڂ̒�������
        /// </summary>
        private void LookAt_Eye()
        {
            //���͂����邩
            if (0.0f < inputWeight_Eye)
            {
                //������̃^�[�Q�b�g�Ƃ̊p�x���擾
                angle_eye = GetHorizontalAngle(lookTarget, virtualHead);

                //���ʂɋ߂��قǑ��x���グ��
                leapSpeed_eye = Time.deltaTime * Mathf.Clamp((searchAngle_Eye / angle_eye), 0, 1);

                //���o���Ȃ珙�X��1��(�Ώۂ̕�������)
                if (searchAngle_Head > angle_head)
                {
                    leapVal_Eye = Mathf.Clamp(leapVal_Eye + leapSpeed_eye, 0.0f, inputWeight_Eye);
                }
                //���o�O�Ȃ珙�X��0��(���ʂ�����)
                else
                {
                    leapVal_Eye = Mathf.Clamp(leapVal_Eye - leapSpeed_eye, 0.0f, inputWeight_Eye);
                }
            }
            else
            {
                leapVal_Eye = 0;//������
            }

            //��̒e�Ȃ��́H
            Vector3 v;
            switch (charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.UnityChan:
                    //���[�J�����W�ɕϊ�
                    v = virtualEye.InverseTransformPoint(lookTarget.position).normalized;
                    result_EyeLook.x = v.x * eye_Amplitude.x * leapVal_Eye;
                    result_EyeLook.y = -v.y * eye_Amplitude.y * leapVal_Eye;
                    //UV���I�t�Z�b�g�𔽉f
                    eyeMat.SetTextureOffset("_BaseMap", result_EyeLook);
                    break;
                case CharaInfoData.CHARATYPE.CandyChan:
                    //���[�J�����W�ɕϊ�
                    v = virtualEye.InverseTransformPoint(lookTarget.position).normalized;
                    result_EyeLook.x = v.x * eye_Amplitude.x * leapVal_Eye;
                    result_EyeLook.y = -v.y * eye_Amplitude.y * leapVal_Eye;
                    //UV���I�t�Z�b�g�𔽉f
                    eyeMat.SetTextureOffset("_BaseMap", result_EyeLook);
                    break;
                case CharaInfoData.CHARATYPE.UnityChanSSU:
                    result_EyeLook.x = eye_Amplitude.x * leapVal_Eye;
                    break;
                case CharaInfoData.CHARATYPE.UnityChanSD:
                    //���[�J�����W�ɕϊ�
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
                    //�ڂɃI�t�Z�b�g�𔽉f
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
                    //�ڂɃI�t�Z�b�g�𔽉f
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

        /// <summary>
        /// ������̃^�[�Q�b�g�Ƃ̊p�x���擾
        /// </summary>
        /// <param name="target"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        private float GetHorizontalAngle(Transform target, Transform origin)
        {
            //��l���̕���(����)
            var playerDirection = (target.position - origin.position).GetHorizontalDirection();
            //�p�x
            return Vector3.Angle(origin.forward.GetHorizontalDirection(), playerDirection);
        }

        private void OnAnimatorIK()
        {
            switch (charaCon.charaInfoData.charaType)
            {
                case CharaInfoData.CHARATYPE.UnityChanSSU:
                    //�S�́A�́A���A��
                    animator.SetLookAtWeight(1.0f, 0.0f, leapVal_Head, result_EyeLook.x);
                    animator.SetLookAtPosition(lookTarget.position);
                    break;
                case CharaInfoData.CHARATYPE.UnityChanKAGURA:
                    //�S�́A�́A���A��
                    animator.SetLookAtWeight(1.0f, 0.0f, leapVal_Head, result_EyeLook.x);
                    animator.SetLookAtPosition(lookTarget.position);
                    break;
                default:
                    //�S�́A�́A���A��
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