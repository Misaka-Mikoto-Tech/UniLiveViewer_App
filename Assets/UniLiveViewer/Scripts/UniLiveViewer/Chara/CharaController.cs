using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace UniLiveViewer
{
    public class CharaController : MonoBehaviour
    {
        public enum CHARASTATE
        {
            NULL = 0,
            MINIATURE,
            HOLD,
            ON_CIRCLE,
            FIELD,
        }
        public enum ANIMATIONMODE
        {
            CLIP = 0,
            VMD,
        }

        public CHARASTATE GetCharaState => charaState;
        [SerializeField] private CHARASTATE charaState = CHARASTATE.NULL;
        public ANIMATIONMODE animationMode = ANIMATIONMODE.CLIP;
        public LipSyncController lipSync;
        public FacialSyncController facialSync;
        [SerializeField] private GameObject lowShadowPrefab;//�ȈՉe
        public List<VRMSpringBone> springBoneList = new List<VRMSpringBone>();//�h����̐ڐG����p
        [HideInInspector]public LookAtController lookAtCon;
        public CharaInfoData charaInfoData;
        private float reScalar = 0;

        [Header("��TimeLine�����Ǘ���")]
        public string bindTrackName = "";
        public RuntimeAnimatorController keepRunAnime;//�A�j���[�V�����������̍ۂɒ��O��Ԃ��L�[�v
        public AnimationClip keepHandL_Anime;//��A�j���[�V����(���肩��߂��ۂɕK�v)
        public AnimationClip keepHandR_Anime;//��A�j���[�V����(���肩��߂��ۂɕK�v)
        [SerializeField] private Transform overrideAnchor = null;//���W�̏㏑���p
        private Animator animator;

        public float CustomScalar
        {
            set
            {
                reScalar = value;
                reScalar = Mathf.Clamp(reScalar, 0.25f, 5.0f);
                transform.localScale = Vector3.one * reScalar;
            }
            get
            {
                if (reScalar == 0) reScalar = GlobalConfig.systemData.InitCharaSize;
                return reScalar;
            }
        }

        void Awake()
        {
            animator = transform.GetComponent<Animator>();
            //Preset�L�����̂�
            if (charaInfoData && charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
            {
                var shadowObj = Instantiate(lowShadowPrefab, transform.position, Quaternion.identity);
                shadowObj.transform.parent = animator.GetBoneTransform(HumanBodyBones.Hips);
                shadowObj.transform.localPosition = Vector3.zero;

                if (GetComponent<LookAtController>() != null) lookAtCon = GetComponent<LookAtController>();
            }
        }


        /// <summary>
        /// ��Ԑݒ�
        /// </summary>
        /// <param name="setState"></param>
        /// <param name="overrideTarget">�Ώۈʒu�ɍ��W�����킹��</param>
        public void SetState(CHARASTATE setState, Transform overrideTarget)
        {
            Vector3 globalScale = Vector3.zero;

            if (overrideTarget) Debug.Log(overrideTarget.name);

            overrideAnchor = overrideTarget;
            charaState = setState;
            switch (charaState)
            {
                case CHARASTATE.NULL:
                    globalScale = Vector3.one;
                    break;
                case CHARASTATE.MINIATURE:
                    globalScale = new Vector3(0.25f, 0.25f, 0.25f);
                    break;
                case CHARASTATE.HOLD:
                    globalScale = new Vector3(0.2f, 0.2f, 0.2f);
                    break;
                case CHARASTATE.ON_CIRCLE:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case CHARASTATE.FIELD:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);

                    ////���̊p�x�𒼂�
                    //if (animationMode == ANIMATIONMODE.VMD)
                    //{
                    //    //transform.GetComponent<VMDPlayer>().ToeIKReset_Full().Forget();
                    //    //transform.GetComponent<VMDPlayer>().ToeIKReset();
                    //}
                    break;
            }

            //���W�̏㏑���ݒ�
            if (overrideAnchor)
            {
                transform.parent = overrideAnchor;

                //�eScale�̉e���𖳎�����ׂɎZ�o
                Vector3 scr = overrideAnchor.lossyScale;
                scr.x = 1 / scr.x;
                scr.y = 1 / scr.y;
                scr.z = 1 / scr.z;

                globalScale.x *= scr.x;
                globalScale.y *= scr.y;
                globalScale.z *= scr.z;
            }
            else transform.parent = null;

            transform.localScale = globalScale * CustomScalar;
        }

        // Update is called once per frame
        void Update()
        {
            //���W�̏㏑��
            if (overrideAnchor)
            {
                transform.position = overrideAnchor.position;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            //�h����̐ڐG�U��
            if (charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                foreach (var springBone in springBoneList)
                {
                    if (springBone.isHit_Any)
                    {
                        if (springBone.isLeft_Any) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, charaInfoData.power, charaInfoData.time);
                        if (springBone.isRight_Any) PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, charaInfoData.power, charaInfoData.time);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// �A�j���[�V�����R���g���[���[��Keep���A��������
        /// </summary>
        public void RemoveRunAnime()
        {
            if (keepRunAnime) return;
            keepRunAnime = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = null;
        }

        /// <summary>
        /// ���������A�j���[�V�����R���g���[���[�����ɖ߂�
        /// </summary>
        public void ReturnRunAnime()
        {
            if (!keepRunAnime) return;
            animator.runtimeAnimatorController = keepRunAnime;
            keepRunAnime = null;
        }
    }
}