using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using NanaCiel;

namespace UniLiveViewer
{
    //���������~��
    [RequireComponent(typeof(AudioSource))]
    public class SpecialFacial : MonoBehaviour
    {
        [Header("����Unity�����p��")]
        //[SerializeField]
        //private SkinnedMeshRenderer face;
        //[SerializeField]
        //private SkinnedMeshRenderer faceTrans;
        private int[] listFaceID;
        private int[] listFaceTransID;

        private bool isAngFace = false;

        //[Header("�����z�w�b�h�p��")]
        //[SerializeField]
        //private string myLayer = "VirtualHead";
        //private Transform virtualHead;

        private CharaController charaCon;
        private LookAtController lookAtCon;
        private bool faceChanging = false;
        private Vector3 dir;
        private RaycastHit rayHit;
        private bool isShockSound = false;

        private AudioSource audioSource;
        [SerializeField]
        private AudioClip[] Sound;
        [SerializeField]
        private AudioClip[] Sound_ANG;
        [SerializeField]
        private AudioClip[] Sound_CONF;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;
            lookAtCon = GetComponent<LookAtController>();

            charaCon = GetComponent<CharaController>();
            if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChan)
            {
                //�W�g��50% + (CONF100% or ANG100%)
                listFaceID = new int[3] { 17, 3, 4 };
                listFaceTransID = new int[3] { 6, 10, 12 };
            }
            else if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.CandyChan)
            {
                listFaceID = new int[3] { 17, 3, 4 };
                listFaceTransID = new int[3] { 12, 3, 5 };
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            audioSource.playOnAwake = false;
            //virtualHead = new GameObject("VirtualHead").transform;
            //virtualHead.parent = transform;
            //virtualHead.gameObject.layer = LayerMask.NameToLayer(myLayer);
            //virtualHead.gameObject.AddComponent(typeof(SphereCollider));
            //var col = virtualHead.GetComponent<SphereCollider>();
            //col.radius = 0.06f;
            //col.isTrigger = true;

            //�����͖��������Ă����A�ݒu��Ԃ̂ݗL����
            this.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            //�}�j���A�����[�h����AnimeCon��keep���̂�
            if (!charaCon.keepRunAnime) return;

            //Lookat���L����Ԃ̎�
            if (charaCon.lookAtCon.inputWeight_Eye <= 0.0f) return;

            ////���w�b�h���L�����̓��ɍ��킹��
            //virtualHead.position = CharaCon.centerEyeAnchor.position;
            //virtualHead.rotation = CharaCon.lastSpineAnchor.rotation;

            if (!faceChanging)
            {
                //�^�[�Q�b�g�������Ⴍ�A�ߐڏ�Ԃ�
                if ((lookAtCon.lookTarget.position.y - transform.position.y) > 1.1f) return;
                if ((transform.position - lookAtCon.lookTarget.position).GetHorizontalDirection().sqrMagnitude < 0.4f)
                {
                    //���[�J�����W�ɕϊ�
                    dir = lookAtCon.virtualHead.InverseTransformPoint(lookAtCon.lookTarget.position).normalized;
                    //���w�b�h���猩�ă^�[�Q�b�g�������낷���
                    if (dir.y < -0.55f && dir.z >= 0.1f)
                    {
                        //�\��ύX
                        if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChan
                            || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.CandyChan)
                        {
                            StartCoroutine(ChangeFace());
                        }
                        else if (charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.VRM_BlendShape
                            || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.VRM_Bone
                            || charaCon.charaInfoData.charaType == CharaInfoData.CHARATYPE.VRM_UV)
                        {
                            StartCoroutine(ChangeFace_VRM());
                        }
                        //�s����
                        audioSource.PlayOneShot(Sound[0]);
                    }
                }
            }
            else
            {
                //�^�[�Q�b�g��������ɋC�Â��Ă��Ȃ����(�V���b�N�����܂�)
                if (!isShockSound)
                {
                    //�^�[�Q�b�g���王����ray���΂�
                    Physics.Raycast(lookAtCon.lookTarget.position, lookAtCon.lookTarget.forward, out rayHit, 2.0f, Parameters.layerMask_VirtualHead);
                    //���w�b�h�Ƀq�b�g���Ă����(�^�[�Q�b�g�������������)
                    if (rayHit.collider && rayHit.collider.transform.root == transform)
                    {
                        isShockSound = true;

                        if (isAngFace)
                        {
                            //ANG��
                            int i = Random.Range(0, Sound_ANG.Length);
                            audioSource.PlayOneShot(Sound_ANG[i]);
                        }
                        else
                        {
                            //CONF��
                            int i = Random.Range(0, Sound_CONF.Length);
                            audioSource.PlayOneShot(Sound_CONF[i]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// VRM�p�E�E�E�Ƃ肠����
        /// </summary>
        public void SetAudioClip_VRM(AudioClip[] clips)
        {
            Sound = new AudioClip[1];
            Sound[0] = clips[0];

            Sound_ANG = new AudioClip[2];
            Sound_ANG[0] = clips[1];
            Sound_ANG[1] = clips[2];

            Sound_CONF = new AudioClip[4];
            Sound_CONF[0] = clips[3];
            Sound_CONF[1] = clips[4];
            Sound_CONF[2] = clips[5];
            Sound_CONF[3] = clips[6];
        }

        /// <summary>
        /// �\��J��(��Unity�����p)
        /// </summary>
        /// <returns></returns>
        private IEnumerator ChangeFace()
        {
            //Conf
            int faceID = listFaceID[1];
            int faceTransID = listFaceTransID[1];
            isAngFace = false;
            if (Random.Range(0, 2) == 1)
            {
                //Ang
                isAngFace = true;
                faceID = listFaceID[2];
                faceTransID = listFaceTransID[2];
            }
            faceChanging = true;

            //��������̂Ń��b�v�V���N���~�߂�
            charaCon.lipSync.enabled = false;
            yield return null;

            //�����\���S�ď��������Ă���
            charaCon.facialSync.AllClear_BlendShape();
            //�����\���S�ď��������Ă���
            charaCon.lipSync.AllClear_BlendShape();
            yield return null;

            float weight = 0;
            //��J��
            for (int i = 0; i <= 10; i++)
            {
                weight = 5 * i;
                //EYE_DEF_C
                charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(listFaceID[0], weight);
                //EL_DEF_C
                charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(listFaceTransID[0], weight);

                weight = 10 * i;
                //random
                charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(faceID, weight);
                charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(faceTransID, weight);

                yield return new WaitForSeconds(0.05f);
            }

            //�s����������������Ȃ��悤�ɍŒ���L�[�v
            yield return new WaitForSeconds(3.0f);

            //���̍����̎��_�܂ŗ��������
            while (dir.y < -0.2f)
            {
                //�L�[�v����������Ă��狭���I��
                if (!charaCon.keepRunAnime) break;
                dir = lookAtCon.virtualHead.InverseTransformPoint(lookAtCon.lookTarget.position).normalized;
                yield return new WaitForSeconds(0.1f);
            }

            //�����
            for (int i = 0; i <= 5; i++)
            {
                weight = 50 - (10 * i);
                //EYE_DEF_C
                charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(listFaceID[0], weight);
                //EL_DEF_C
                charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(listFaceTransID[0], weight);

                weight = 100 - (20 * i);
                //random
                charaCon.facialSync.uniSkin[0].SetBlendShapeWeight(faceID, weight);
                charaCon.facialSync.uniSkin[1].SetBlendShapeWeight(faceTransID, weight);

                yield return new WaitForSeconds(0.02f);
            }

            //���b�v�V���N��߂�
            charaCon.lipSync.enabled = true;
            isShockSound = false;
            faceChanging = false;
        }

        /// <summary>
        /// �\��J��(VRM�p)
        /// </summary>
        /// <returns></returns>
        private IEnumerator ChangeFace_VRM()
        {
            //Conf
            VRM.BlendShapePreset preset = VRM.BlendShapePreset.Sorrow;
            isAngFace = false;
            if (Random.Range(0, 2) == 1)
            {
                //Ang
                isAngFace = true;
                preset = VRM.BlendShapePreset.Angry;
            }
            faceChanging = true;

            //��������̂Ń��b�v�V���N���~�߂�
            charaCon.lipSync.enabled = false;
            yield return null;

            //�\��ύX������̂Ń}�j���A��mode�ɂ���
            charaCon.facialSync.isManualControl = true;

            //�����\���S�ď��������Ă���
            //charaCon.facialSync.AllClear_BlendShape();
            //charaCon.lipSync.AllClear_BlendShape();

            float weight = 0;
            //��J��
            for (int i = 0; i <= 10; i++)
            {
                weight = 0.1f * i;
                //random
                charaCon.facialSync.SetBlendShape(preset, weight);
                yield return new WaitForSeconds(0.05f);
            }

            //�s����������������Ȃ��悤�ɍŒ���L�[�v
            yield return new WaitForSeconds(3.0f);

            //���̍����̎��_�܂ŗ��������
            while (dir.y < -0.2f)
            {
                //�L�[�v����������Ă��狭���I��
                if (!charaCon.keepRunAnime) break;
                dir = lookAtCon.virtualHead.InverseTransformPoint(lookAtCon.lookTarget.position).normalized;
                yield return new WaitForSeconds(0.1f);
            }

            //�����
            for (int i = 0; i <= 5; i++)
            {
                weight = 1 - (0.2f * i);
                //random
                charaCon.facialSync.SetBlendShape(preset, weight);

                yield return new WaitForSeconds(0.02f);
            }

            //���b�v�V���N��߂�
            charaCon.lipSync.enabled = true;
            isShockSound = false;
            faceChanging = false;

            //�\��ύX���[�h��߂�
            charaCon.facialSync.isManualControl = false;
        }
    }

}