using UnityEngine;
using System;

namespace UniLiveViewer
{
    //��肪�G(�s�����)
    public class OVRGrabber_UniLiveViewer : OVRGrabber_Custom
    {
        public enum HandState
        {
            DEFAULT,//�������Ă��Ȃ��A��������ł��Ȃ�
            GRABBED_CHARA,//�L������͂�ł���
            GRABBED_ITEM,//�A�C�e����͂�ł���
            GRABBED_OTHER,//�L�����ȊO��͂�ł���
            SUMMONCIRCLE,//�����wON
            CHARA_ONCIRCLE//�L������͂�ł��邩�����wON
        }

        //�Ή�����Z���N�^�[
        public LineSelector lineSelector = null;

        public bool IsSummonCircle { get; private set; } = false;
        public HandState handState = HandState.DEFAULT;

        private TimelineController timeline = null;
        private GeneratorPortal generatorPortal;
        public Transform handMeshRoot;

        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//�͂�,����,����,�폜

        public event Action<OVRGrabber_UniLiveViewer> OnSummon;
        public event Action<OVRGrabber_UniLiveViewer> OnGrabItem;
        public event Action<OVRGrabber_UniLiveViewer> OnGrabEnd;

        public Vector3 GetGripPoint
        {
            get { return m_gripTransform.position; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene") return;

            generatorPortal = GameObject.FindGameObjectWithTag("GeneratorPortal").gameObject.GetComponent<GeneratorPortal>();
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;
        }

        public override void Update()
        {
            base.Update();

            //�͂݁��Z���N�^�[�\���Ȃ�Z���N�^�[���샂�[�h
            if (handState == HandState.SUMMONCIRCLE)
            {
                //�F�X�V
                lineSelector.SetMaterial(false);
            }
        }

        /// <summary>
        /// �����I�Ɏ���
        /// </summary>
        public void ForceGrabBegin(OVRGrabbable_Custom grabbedObj)
        {
            if(m_grabbedObj && m_grabbedObj == grabbedObj)
            {
                //���Z�b�g�ړI�ň�U����
                OffhandGrabbed(m_grabbedObj);
            }

            //�R���C�_�[�����w��I�u�W�F�N�g�݂̂ɂ���
            m_grabCandidates.Clear();
            m_grabCandidates[grabbedObj] = 0;

            //�͂ݒ���
            GrabBegin();
            m_grabCandidates.Clear();
        }

        public void FoeceGrabEnd()
        {
            if (m_grabbedObj) GrabEnd();
        }

        /// <summary>
        /// �݂͂̊�{�@�\�A�͂񂾃I�u�W�F�N�g�̍єz�A�͂݃��[�V�����ɂ��폜�@�\
        /// </summary>
        protected override void GrabBegin()
        {
            //��{�I�Ȏ�ɐG��Ă�����̂�͂ޏ���
            base.GrabBegin();

            //������͂񂾏ꍇ
            if (grabbedObject)
            {
                //�L�����ȊO
                if (!grabbedObject.gameObject.CompareTag(Parameters.tag_GrabChara))
                {
                    // TODO:�����d�l������蒼������
                    //����݂͂̓A�C�e���Ƃ����d�l��
                    if (grabbedObject.isBothHandsGrab)
                    {
                        handState = HandState.GRABBED_ITEM;
                        grabbedObject.GetComponent<MeshRenderer>().enabled = true;
                        grabbedObject.transform.parent = transform;
                        timeline.SetActive_AttachPoint(true);

                        //item�͂�
                        OnGrabItem?.Invoke(this);
                    }
                    else
                    {
                        handState = HandState.GRABBED_OTHER;
                    }
                }
                //�L����
                else
                {
                    //�����w�̏��
                    if (IsSummonCircle)
                    {
                        //�����w�̏�ɏ悹��
                        var chara = grabbedObject.gameObject.GetComponent<CharaController>();
                        chara.SetState(CharaController.CHARASTATE.ON_CIRCLE, lineSelector.LineEndAnchor);

                        handState = HandState.CHARA_ONCIRCLE;
                    }
                    else
                    {
                        //��Ɏ�������
                        var chara = grabbedObject.gameObject.GetComponent<CharaController>();
                        chara.SetState(CharaController.CHARASTATE.HOLD, null);

                        handState = HandState.GRABBED_CHARA;
                    }
                    //�͂݉�
                    audioSource.PlayOneShot(Sound[0]);
                }
            }
            //�͂�ł��Ȃ��ꍇ
            else
            {
                //�����w�̏��
                if (IsSummonCircle)
                {
                    //�Z���N�^�[�ɃI�u�W�F�N�g���G��Ă��邩(layer�I�ɃL����)
                    if (!lineSelector.hitCollider.collider) return;

                    //�ΏۃI�u�W�F�N�g���t�B�[���h����폜����
                    timeline.DeletebindAsset(lineSelector.hitCollider.transform.GetComponent<CharaController>());

                    //�F�X�V
                    lineSelector.SetMaterial(true);

                    //�폜��
                    audioSource.PlayOneShot(Sound[3]);
                    handState = HandState.SUMMONCIRCLE;
                }
            }
        }

        /// <summary>
        /// ������{�@�\�A�������I�u�W�F�N�g�̍єz
        /// </summary>
        protected override void GrabEnd()
        {
            if (!m_grabbedObj)
            {
                //�݂͂𗣂���{�@�\
                base.GrabEnd();
            }
            else
            {
                CharaController keepChara = null;

                //��\���Ȃ�\���ɖ߂�
                if (!handMeshRoot.gameObject.activeSelf) handMeshRoot.gameObject.SetActive(true);

                switch (handState)
                {
                    //�L�����ȊO��͂�ł����
                    case HandState.GRABBED_OTHER:
                        //�݂͂𗣂���{�@�\
                        base.GrabEnd();
                        handState = HandState.DEFAULT;
                        break;
                    case HandState.GRABBED_ITEM:
                        var grabbedObj = m_grabbedObj;//������m�F�̈׃L�[�v
                        grabbedObj.transform.parent = null;
                        //�݂͂𗣂���{�@�\
                        base.GrabEnd();
                        handState = HandState.DEFAULT;
                        //�܂��߂܂�Ă��Ȃ���Θg��\��
                        if(!grabbedObj.isGrabbed) grabbedObj.GetComponent<MeshRenderer>().enabled = false;
                        break;
                    //�L������͂�ł���
                    case HandState.GRABBED_CHARA:
                        keepChara = m_grabbedObj.gameObject.GetComponent<CharaController>();
                        //�݂͂𗣂���{�@�\
                        base.GrabEnd();
                        //Portal�ɖ߂�
                        PortalBack(keepChara);
                        //������
                        audioSource.PlayOneShot(Sound[1]);
                        handState = HandState.DEFAULT;
                        break;
                    //�L������͂�ł��邩�����wON
                    case HandState.CHARA_ONCIRCLE:
                        keepChara = m_grabbedObj.gameObject.GetComponent<CharaController>();
                        //�݂͂𗣂���{�@�\
                        base.GrabEnd();

                        //�t���[�g���b�N���Ȃ����Portal�ɖ߂�
                        string freeTrack;
                        if (!timeline.isFreeTrack(out freeTrack))
                        {
                            PortalBack(keepChara);
                            //������
                            audioSource.PlayOneShot(Sound[1]);
                            return;
                        }

                        //�Z���N�^�[�̍��W�Ɗp�x���擾
                        Vector3 pos = lineSelector.LineEndAnchor.position;
                        Vector3 eulerAngles = lineSelector.LineEndAnchor.localRotation.eulerAngles;

                        //�ڍs�Ɏ��s�Ȃ�Portal�ɖ߂�
                        if (!timeline.TransferPlayableAsset(keepChara, freeTrack, pos, eulerAngles))
                        {
                            PortalBack(keepChara);
                            //������
                            audioSource.PlayOneShot(Sound[1]);
                            return;
                        }

                        //�L�����̏�Ԃ��t�B�[���h�ɐݒ�
                        keepChara.SetState(CharaController.CHARASTATE.FIELD, null);
                        //layer��ς���
                        keepChara.gameObject.layer = Parameters.layerNo_FieldObject;

                        if (keepChara.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChanSSU)
                        {
                            //�����ۃ^�b�`����L���ɂ���
                            keepChara.GetComponent<TouchSound>().enabled = true;
                        }
                        else
                        {
                            //�s��̈ז�����
                            //���ꉉ�o�̃t���O��L���ɂ���
                            //keepChara.GetComponent<SpecialFacial>().enabled = true;
                        }

                        //������
                        audioSource.PlayOneShot(Sound[2]);

                        handState = HandState.SUMMONCIRCLE;

                        //��������
                        OnSummon?.Invoke(this);

                        break;
                    default:
                        //�݂͂𗣂���{�@�\
                        base.GrabEnd();
                        handState = HandState.DEFAULT;
                        break;
                }
            }
            //������
            OnGrabEnd?.Invoke(this);
        }


        /// <summary>
        /// �͂�ł������̂��|�[�^���ɖ߂�
        /// </summary>
        /// <param name="keepChara"></param>
        private void PortalBack(CharaController keepChara)
        {
            // �|�[�^���ɖ߂�
            if (generatorPortal.gameObject.activeSelf)
            {
                keepChara.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
            }
            ////�|�[�^���g�̃L������null�������@�s�v
            //else timeline.NewAssetBinding_Portal(null);
        }

        protected override void OffhandGrabbed(OVRGrabbable_Custom grabbable)
        {
            base.OffhandGrabbed(grabbable);

            handState = HandState.DEFAULT;
        }

        /// <summary>
        /// �Z���N�^�[�̗L���E������Ԃ�؂�ւ���
        /// </summary>
        public void SelectorChangeEnabled()
        {
            CharaController chara = null;

            //���݂̎�̏�Ԃŕ���
            switch (handState)
            {
                //�L������͂�ł��邩�����wON
                case HandState.CHARA_ONCIRCLE:
                    //�茳�ɖ߂�
                    handState = HandState.GRABBED_CHARA;
                    chara = grabbedObject.gameObject.GetComponent<CharaController>();
                    chara.SetState(CharaController.CHARASTATE.HOLD, null);
                    break;
                //�L������͂�ł���
                case HandState.GRABBED_CHARA:
                    //�����w��ɐݒu
                    handState = HandState.CHARA_ONCIRCLE;
                    chara = grabbedObject.gameObject.GetComponent<CharaController>();
                    chara.SetState(CharaController.CHARASTATE.ON_CIRCLE, lineSelector.LineEndAnchor);
                    break;
                //�������Ă��Ȃ�
                case HandState.DEFAULT:
                    handState = HandState.SUMMONCIRCLE;
                    break;
                //�����wON
                case HandState.SUMMONCIRCLE:
                    handState = HandState.DEFAULT;
                    break;
            }

            //�����w�̏�Ԃ𔽓]
            IsSummonCircle = !lineSelector.gameObject.activeSelf;
            lineSelector.gameObject.SetActive(IsSummonCircle);
        }
    }
}
