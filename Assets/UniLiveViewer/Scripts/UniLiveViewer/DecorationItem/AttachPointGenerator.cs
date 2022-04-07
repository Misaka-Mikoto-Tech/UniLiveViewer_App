using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class AttachPointGenerator : MonoBehaviour
    {
        public AttachPoint anchorPointPrefab;
        public List<AttachPoint> anchorList = new List<AttachPoint>();
        private Animator anime;
        private Dictionary<HumanBodyBones, float> dicAttachPoint = new Dictionary<HumanBodyBones, float>();

        public bool isCustomize = false;

        public bool isswitch = false;
        public float subnnl;

        private CharaController charaCon;
        private TimelineController timeline;

        public float height = 0;//�g���͂Ƃ肠�����}��邪�A�������܂������Ȃ��Ɩ��Ӗ�(�����p���o�O���Ă�z�������Ȃ��Ƃ����Ȃ�)

        // Start is called before the first frame update
        void Start()
        {
            anime = GetComponent<Animator>();
            charaCon = transform.GetComponent<CharaController>();
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();

            //�g����}��(UI��x�[�X��0.15�`0.35���炢)
            //height = anime.GetBoneTransform(HumanBodyBones.Head).position.y - anime.GetBoneTransform(HumanBodyBones.RightFoot).position.y;
            //�����̖ʓ|�Ȃ̂ō���(UI��x�[�X��0.07�`0.12���炢)
            height = anime.GetBoneTransform(HumanBodyBones.Head).position.y - anime.GetBoneTransform(HumanBodyBones.Spine).position.y;


            //�񋹁`���܂ł̋������Z����΃~�j�L�����ƔF�肷��
            //var dir = anime.GetBoneTransform(HumanBodyBones.Head).position - anime.GetBoneTransform(HumanBodyBones.Neck).parent.position;
            //if (dir.sqrMagnitude < 0.035f) isMiniChara = true;

            if (isCustomize)
            {
                dicAttachPoint = new Dictionary<HumanBodyBones, float>()
            {
                //offset���W
                { HumanBodyBones.LeftHand, 0f},
                { HumanBodyBones.RightHand,  0f},
                { HumanBodyBones.Head,0.16f},
                { HumanBodyBones.Chest,0f},
                //{ HumanBodyBones.Spine,0f}//��
            };

                foreach (var e in dicAttachPoint)
                {
                    //�A�^�b�`�I�u�W�F����
                    var attachPoint = Instantiate(anchorPointPrefab.gameObject, transform.position, Quaternion.identity);
                    var attachPointScript = attachPoint.GetComponent<AttachPoint>();
                    attachPointScript.myCharaCon = charaCon;

                    //�p�����[�^�ݒ�
                    attachPoint.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), e.Key);
                    if (e.Key == HumanBodyBones.Neck)
                    {
                        Transform chest = anime.GetBoneTransform(e.Key).parent;//��̐e�{�[��(�C�ӂȂ̂łȂ��\��������)
                        if (!chest) chest = anime.GetBoneTransform(HumanBodyBones.Head).parent;//�m���ɂ��铪�̐e�{�[��
                                                                                               //�Z�b�g����
                        attachPoint.transform.parent = chest;
                    }
                    else attachPoint.transform.parent = anime.GetBoneTransform(e.Key);
                    attachPoint.transform.localRotation = Quaternion.identity;

                    switch (e.Key)
                    {
                        case HumanBodyBones.Head:
                            attachPoint.transform.localPosition = new Vector3(0, 0, e.Value);
                            attachPoint.transform.localScale = Vector3.one * 0.5f;
                            break;
                        case HumanBodyBones.LeftHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;
                        case HumanBodyBones.RightHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;

                        default:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.35f;
                            break;
                    }
                    anchorList.Add(attachPointScript);
                }
            }
            else
            {
                dicAttachPoint = new Dictionary<HumanBodyBones, float>()
            {
                //offset���W
                { HumanBodyBones.LeftHand, 0.05f},
                { HumanBodyBones.RightHand,  0.05f},
                { HumanBodyBones.Head,0.1f},
                { HumanBodyBones.Neck,0.1f},//��(���f���ɂ���č\�����قȂ�̂ň�Ԉ��S�Ɏ�̐e���擾����)
                { HumanBodyBones.Spine,-0.03f}//��
            };

                foreach (var e in dicAttachPoint)
                {
                    //�A�^�b�`�I�u�W�F����
                    var attachPoint = Instantiate(anchorPointPrefab.gameObject, transform.position, Quaternion.identity);
                    var attachPointScript = attachPoint.GetComponent<AttachPoint>();
                    attachPointScript.myCharaCon = charaCon;

                    //�p�����[�^�ݒ�
                    attachPoint.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), e.Key);
                    if (e.Key == HumanBodyBones.Neck)
                    {
                        Transform chest = anime.GetBoneTransform(e.Key)?.parent;//��̐e�{�[��(�C�ӂȂ̂łȂ��\��������)
                        if (!chest) chest = anime.GetBoneTransform(HumanBodyBones.Head).parent;//�m���ɂ��铪�̐e�{�[��
                                                                                               //�Z�b�g����
                        attachPoint.transform.parent = chest;
                    }
                    else attachPoint.transform.parent = anime.GetBoneTransform(e.Key);
                    attachPoint.transform.localRotation = Quaternion.identity;

                    switch (e.Key)
                    {
                        //case HumanBodyBones.Head:
                        //    attachPoint.transform.localPosition = Vector3.zero;
                        //    attachPoint.transform.position += new Vector3(0, e.Value, 0);
                        //    attachPoint.transform.localScale = Vector3.one * 0.45f;
                        //    break;
                        case HumanBodyBones.Neck:
                            if (height >= 0.11f) attachPoint.transform.localPosition = new Vector3(0, e.Value + 0.01f, 0);
                            else attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.4f;
                            break;
                        case HumanBodyBones.LeftHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;
                        case HumanBodyBones.RightHand:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.2f;
                            break;
                        default:
                            attachPoint.transform.localPosition = new Vector3(0, e.Value, 0);
                            //attachPoint.transform.position += new Vector3(0, e.Value, 0);
                            attachPoint.transform.localScale = Vector3.one * 0.35f;
                            break;
                    }
                    anchorList.Add(attachPointScript);
                }
            }

            //���������Ă���
            SetActive_AttachPoint(false);
        }

        public void Update()
        {
            //����̃A�^�b�`�|�C���g�̃A�C�e�����m�F
            if (anchorList[0].transform.childCount == 0)
            {
                //�����Ă������������
                if (charaCon.keepHandL_Anime) timeline.SwitchHandType(charaCon, false, true);
            }
            //�E��̃A�^�b�`�|�C���g�̃A�C�e�����m�F
            if (anchorList[1].transform.childCount == 0)
            {
                //�����Ă������������
                if (charaCon.keepHandR_Anime) timeline.SwitchHandType(charaCon, false, false);
            }
        }

        public void SetActive_AttachPoint(bool isActive)
        {
            foreach (var anchor in anchorList)
            {
                anchor.SetActive(isActive);
            }
        }
    }

}