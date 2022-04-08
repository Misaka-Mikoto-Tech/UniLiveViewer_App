using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniLiveViewer 
{
    //TODO:�������A��������
    public class PlayerStateManager : MonoBehaviour
    {
        [Header("��{")]
        [SerializeField] private SimpleCapsuleWithStickMovement simpleCapsuleWithStickMovement = null;
        //public bool isOperation = false;//�J������֎~�p�t���O
        private static bool isSummonCircle = false;
        public static bool isGrabbedChara_OnCircle = false;
        private TimelineController timeline;
        //[SerializeField] private OVRScreenFade screenFade;

        [Header("�͂�")]
        [SerializeField] private OVRGrabber_UniLiveViewer[] ovrGrabber = null;//���E                                                                      
        //����Œ͂�
        private OVRGrabbable_Custom bothHandsGrabObj;
        private Vector3 initBothHandsDistance;
        private Transform bothHandsCenterAnchor;

        [Header("UI�֌W")]
        [SerializeField] private MoveUI moveUI;
        [SerializeField] private Transform handUI;
        private bool isMoveUI = false;
        private bool isHandUI = false;
        private TextMesh textMesh_CamHei;
        private CharacterCameraConstraint_Custom charaCam;
        [SerializeField] private Transform[] crossUI = new Transform[2];
        private TextMesh[] textMesh_cross = new TextMesh[2];
        [Header("�g�p�L�[")]
        //UI
        [SerializeField] private KeyCode uiKey_win = KeyCode.U;
        [SerializeField] private OVRInput.RawButton[] uiKey_quest = { OVRInput.RawButton.Y, OVRInput.RawButton.B };
        //��]�Ɏg�p����L�[
        [SerializeField]
        private OVRInput.RawButton[] roteKey_LCon = {
            OVRInput.RawButton.LThumbstickLeft,OVRInput.RawButton.LThumbstickRight
        };
        [SerializeField]
        private OVRInput.RawButton[] roteKey_Rcon = {
            OVRInput.RawButton.RThumbstickLeft,OVRInput.RawButton.RThumbstickRight
        };
        //�T�C�Y�ύX�Ɏg�p����L�[
        [SerializeField]
        private OVRInput.RawButton[] resizeKey_LCon = {
            OVRInput.RawButton.LThumbstickDown,OVRInput.RawButton.LThumbstickUp
        };

        [SerializeField]
        private OVRInput.RawButton[] resizeKey_RCon = {
            OVRInput.RawButton.RThumbstickDown,OVRInput.RawButton.RThumbstickUp
        };
        //���C���Z���N�^�[�\���ؑփL�[
        [SerializeField]
        private OVRInput.RawButton[] lineOnKey = {
            OVRInput.RawButton.X, OVRInput.RawButton.A
        };
        //�A�^�b�`�Ɏg�p
        [SerializeField]
        private OVRInput.RawButton[] actionKey = {
            OVRInput.RawButton.LIndexTrigger, OVRInput.RawButton.RIndexTrigger
        };

        [Header("�T�E���h")]
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//UI�J��,UI����

        public OVRGrabbable_Custom[] bothHandsCandidate = new OVRGrabbable_Custom[2];

        private CancellationToken cancellation_token;
        private static PlayerStateManager instance = null;

        private void Awake()
        {
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;

            charaCam = GetComponent<CharacterCameraConstraint_Custom>();
            textMesh_CamHei = handUI.GetChild(0).GetComponent<TextMesh>();

            for (int i = 0; i < crossUI.Length; i++)
            {
                textMesh_cross[i] = crossUI[i].GetChild(0).GetComponent<TextMesh>();
            }

            //����͂ݗp
            foreach (var hand in ovrGrabber)
            {
                hand.OnSummon += ChangeSummonCircle;
                hand.OnGrabItem += BothHandsCandidate;
                hand.OnGrabEnd += BothHandsGrabEnd;
            }
            bothHandsCenterAnchor = new GameObject("BothHandsCenter").transform;

            cancellation_token = this.GetCancellationTokenOnDestroy();
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            //��\��������
            handUI.gameObject.SetActive(isHandUI);

            for (int i = 0; i < crossUI.Length; i++)
            {
                crossUI[i].gameObject.SetActive(false);
            }

            this.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            //�������ς݂łȂ���Ώ������Ȃ�
            //if (!isOperation) return;

            isGrabbedChara_OnCircle = false;
            isSummonCircle = false;

            //���萧��
            if (ovrGrabber[0].handState == OVRGrabber_UniLiveViewer.HandState.CHARA_ONCIRCLE)
            {
                isGrabbedChara_OnCircle = true;
                crossUI[0].gameObject.SetActive(true);

                //���荶��]
                if (OVRInput.GetDown(roteKey_LCon[0]))
                {
                    ovrGrabber[0].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                    //��]��
                    audioSource.PlayOneShot(Sound[2]);
                }
                //����E��]
                else if (OVRInput.GetDown(roteKey_LCon[1]))
                {
                    ovrGrabber[0].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                    //��]��
                    audioSource.PlayOneShot(Sound[2]);
                }

                //����k��
                if (OVRInput.Get(resizeKey_LCon[0]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += -0.005f;
                }
                //����g��
                else if (OVRInput.Get(resizeKey_LCon[1]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += 0.005f;
                }

                textMesh_cross[0].text = $"{timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar:0.00}";
            }
            else
            {
                crossUI[0].gameObject.SetActive(false);
            }

            if (ovrGrabber[1].handState == OVRGrabber_UniLiveViewer.HandState.CHARA_ONCIRCLE)
            {
                isGrabbedChara_OnCircle = true;
                crossUI[1].gameObject.SetActive(true);

                //�E�荶��]
                if (OVRInput.GetDown(roteKey_Rcon[0]))
                {
                    ovrGrabber[1].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                    //��]��
                    audioSource.PlayOneShot(Sound[2]);
                }
                //�E��E��]
                else if (OVRInput.GetDown(roteKey_Rcon[1]))
                {
                    ovrGrabber[1].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                    //��]��
                    audioSource.PlayOneShot(Sound[2]);
                }

                //�E��k��
                if (OVRInput.Get(resizeKey_RCon[0]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += -0.01f;
                }
                //�E��g��
                else if (OVRInput.Get(resizeKey_RCon[1]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += 0.01f;
                }

                textMesh_cross[1].text = $"{timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar:0.00}";
            }
            else
            {
                crossUI[1].gameObject.SetActive(false);
            }

            //�L�����������w�ɃZ�b�g����Ă����
            if (isGrabbedChara_OnCircle)
            {
                //�ړ��ƕ����]���𖳌���
                simpleCapsuleWithStickMovement.EnableLinearMovement = false;
                simpleCapsuleWithStickMovement.EnableRotation = false;
            }
            //�������n
            else
            {
                simpleCapsuleWithStickMovement.EnableRotation = true;
                if (!isHandUI) simpleCapsuleWithStickMovement.EnableLinearMovement = true;

                //�n���hUI�o����
                if (isHandUI)
                {
                    //�A�i���O�X�e�B�b�N�ŃJ�����ʒu����
                    if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp))
                    {
                        charaCam.HeightOffset = Mathf.Clamp(charaCam.HeightOffset + 0.05f, 0f, 1.5f);
                        textMesh_CamHei.text = $"{charaCam.HeightOffset:0.00}";
                    }
                    if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickDown))
                    {
                        charaCam.HeightOffset = Mathf.Clamp(charaCam.HeightOffset - 0.05f, 0f, 1.5f);
                        textMesh_CamHei.text = $"{charaCam.HeightOffset:0.00}";
                    }
                }
            }

            //���C���Z���N�^�[�؂�ւ�
            for (int i = 0; i < lineOnKey.Length; i++)
            {
                if (OVRInput.GetDown(lineOnKey[i]))
                {
                    ovrGrabber[i].SelectorChangeEnabled();
                    Click_SummonCircle();
                }
            }

            //UI�\��
            if (OVRInput.GetDown(uiKey_quest[1]) || Input.GetKeyDown(uiKey_win))
            {
                SwitchUI();
            }
            if (OVRInput.GetDown(uiKey_quest[0]))
            {
                SwitchHandUI();
            }

            //�A�C�e�����A�^�b�`����
            for (int i = 0; i < actionKey.Length; i++)
            {
                if (OVRInput.GetDown(actionKey[i]))
                {
                    var grabObj = ovrGrabber[i].grabbedObject;
                    if (grabObj && grabObj.isBothHandsGrab)
                    {
                        ovrGrabber[i].FoeceGrabEnd();

                        //�A�^�b�`�Ώۂ��肩�}�j���A�����[�h
                        if (grabObj.hitCollider && timeline.isManualMode())
                        {
                            //��Ȃ爬�点��
                            if (grabObj.hitCollider.name.Contains("Hand"))
                            {
                                var targetChara = grabObj.hitCollider.GetComponent<AttachPoint>().myCharaCon;
                                timeline.SwitchHandType(targetChara, true, grabObj.hitCollider.name.Contains("Left"));
                            }

                            //�A�^�b�`����
                            grabObj.AttachToHitCollider();
                            audioSource.PlayOneShot(Sound[3]);
                        }
                        //�A�^�b�`�悪�Ȃ���΍폜
                        else
                        {
                            Destroy(grabObj.gameObject);
                            audioSource.PlayOneShot(Sound[4]);
                        }


                        //���肪�t���[��
                        if (!ovrGrabber[0].grabbedObject && !ovrGrabber[1].grabbedObject)
                        {
                            //�A�^�b�`�|�C���g�𖳌���
                            timeline.SetActive_AttachPoint(false);
                        }
                    }
                }
            }
        }

        private void LateUpdate()
        {
            //����Œ͂ރI�u�W�F�N�g������΍��W���㏑������
            if (bothHandsGrabObj)
            {
                //����̒��ԍ��W
                Vector3 bothHandsDistance = (ovrGrabber[1].GetGripPoint - ovrGrabber[0].GetGripPoint);
                bothHandsCenterAnchor.localScale = Vector3.one * bothHandsDistance.sqrMagnitude / initBothHandsDistance.sqrMagnitude;
                bothHandsCenterAnchor.position = bothHandsDistance * 0.5f + ovrGrabber[0].GetGripPoint;
                bothHandsCenterAnchor.forward = (ovrGrabber[0].transform.forward + ovrGrabber[1].transform.forward) * 0.5f;
            }
        }

        /// <summary>
        /// �����w�̏�Ԃ��X�C�b�`
        /// </summary>
        /// <param name="target"></param>
        public void ChangeSummonCircle(OVRGrabber_UniLiveViewer target)
        {
            //���C���Z���N�^�[�؂�ւ�
            for (int i = 0; i < lineOnKey.Length; i++)
            {
                if (ovrGrabber[i] == target)
                {
                    ovrGrabber[i].SelectorChangeEnabled();
                    Click_SummonCircle();
                    break;
                }
            }
        }

        /// <summary>
        /// ����͂݌��Ƃ��ēo�^
        /// </summary>
        /// <param name="newHand"></param>
        private void BothHandsCandidate(OVRGrabber_UniLiveViewer newHand)
        {
            if (newHand == ovrGrabber[0])
            {
                bothHandsCandidate[0] = ovrGrabber[0].grabbedObject;

                //���O�܂Ŕ��΂̎�Œ͂�ł����I�u�W�F�N�g�Ȃ�
                if (bothHandsCandidate[1] == bothHandsCandidate[0])
                {

                    //����p�I�u�W�F�N�g�Ƃ��ăZ�b�g
                    bothHandsGrabObj = bothHandsCandidate[0];
                    //�����l���L�^
                    initBothHandsDistance = (ovrGrabber[1].GetGripPoint - ovrGrabber[0].GetGripPoint);
                    bothHandsCenterAnchor.position = initBothHandsDistance * 0.5f + ovrGrabber[0].GetGripPoint;
                    bothHandsCenterAnchor.forward = (ovrGrabber[0].transform.forward + ovrGrabber[1].transform.forward) * 0.5f;
                    bothHandsGrabObj.transform.parent = bothHandsCenterAnchor;
                }
            }
            else if (newHand == ovrGrabber[1])
            {
                bothHandsCandidate[1] = ovrGrabber[1].grabbedObject;

                //���O�܂Ŕ��΂̎�Œ͂�ł����I�u�W�F�N�g�Ȃ�
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //����p�I�u�W�F�N�g�Ƃ��ăZ�b�g
                    bothHandsGrabObj = bothHandsCandidate[1];
                    //�����l���L�^
                    initBothHandsDistance = (ovrGrabber[1].GetGripPoint - ovrGrabber[0].GetGripPoint);
                    bothHandsCenterAnchor.position = initBothHandsDistance * 0.5f + ovrGrabber[0].GetGripPoint;
                    bothHandsCenterAnchor.forward = (ovrGrabber[0].transform.forward + ovrGrabber[1].transform.forward) * 0.5f;
                    bothHandsGrabObj.transform.parent = bothHandsCenterAnchor;
                }
            }
        }

        /// <summary>
        /// ���΂̎�Ŏ�������
        /// </summary>
        /// <param name="releasedHand"></param>
        private void BothHandsGrabEnd(OVRGrabber_UniLiveViewer releasedHand)
        {
            //����ɉ����Ȃ���Ώ������Ȃ�
            if (!bothHandsCandidate[0] && !bothHandsCandidate[1]) return;

            //������
            if (releasedHand == ovrGrabber[0])
            {
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //�����I�Ɏ�������
                    ovrGrabber[1].ForceGrabBegin(bothHandsGrabObj);
                }
                bothHandsCandidate[0] = null;
            }
            else if (releasedHand == ovrGrabber[1])
            {
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //�����I�Ɏ�������
                    ovrGrabber[0].ForceGrabBegin(bothHandsGrabObj);
                }
                bothHandsCandidate[1] = null;
            }
            //����͏I��
            if (bothHandsGrabObj)
            {
                bothHandsGrabObj.transform.parent = null;
                bothHandsCenterAnchor.localScale = Vector3.one;
                bothHandsGrabObj = null;
            }

            //���肪�t���[��
            if (!bothHandsCandidate[0] && !bothHandsCandidate[1])
            {
                //�A�^�b�`�|�C���g�𖳌���
                timeline.SetActive_AttachPoint(false);
            }
        }

        public void SwitchUI()
        {
            //UI�\���̐؂�ւ�
            isMoveUI = !moveUI.gameObject.activeSelf;
            moveUI.gameObject.SetActive(isMoveUI);

            //�\����
            if (isMoveUI) audioSource.PlayOneShot(Sound[0]);
            //��\����
            else audioSource.PlayOneShot(Sound[1]);
        }

        /// <summary>
        /// �J�����̍���UI
        /// </summary>
        public void SwitchHandUI()
        {
            //UI�\���̐؂�ւ�
            isHandUI = !isHandUI;
            handUI.gameObject.SetActive(isHandUI);

            if (isHandUI)
            {
                //textMesh_CamHei.text = charaCam.HeightOffset.ToString("0.00");
                textMesh_CamHei.text = $"{charaCam.HeightOffset:0.00}";
                //�ړ��𖳌���
                simpleCapsuleWithStickMovement.EnableLinearMovement = false;
            }
            else
            {
                //�ړ��𖳌���
                simpleCapsuleWithStickMovement.EnableLinearMovement = true;
            }

            //�\����
            if (isHandUI) audioSource.PlayOneShot(Sound[0]);
            //��\����
            else audioSource.PlayOneShot(Sound[1]);
        }

        /// <summary>
        /// �ǂ��炩�̎�őΏۃ^�O�̃I�u�W�F�N�g��͂�ł��邩
        /// </summary>
        public bool CheckGrabbing()
        {
            for (int i = 0; i < ovrGrabber.Length; i++)
            {
                if (!ovrGrabber[i].grabbedObject) continue;
                if (ovrGrabber[i].grabbedObject.gameObject.CompareTag(Parameters.tag_GrabSliderVolume))
                {
                    return true;
                }
            }
            return false;
        }

        private void Click_SummonCircle()
        {
            //�����ꂩ�̏����w���o�����Ă��邩�H
            isSummonCircle = false;
            foreach (var e in ovrGrabber)
            {
                if (e.IsSummonCircle)
                {
                    isSummonCircle = true;
                    break;
                }
            }
            //�K�C�h�̕\����؂�ւ���
            timeline.SetCharaMeshGuide(isSummonCircle);
        }


        /// <summary>
        /// Player�C���X�^���X�ɃR���g���[���[�U�����w��
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">���g��0~1(1�̕����@�ׂȋC������)</param>
        /// <param name="amplitude">�U�ꕝ0~1(0�Œ�~)</param>
        /// <param name="time">�U�����ԁA���2�b�炵��</param>
        public static void ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            if (!GlobalConfig.isControllerVibration) return;

            if (instance) instance.UniTask_ControllerVibration(touch, frequency, amplitude, time);

            //Task.Run(async () =>
            //{
            //    //�U���J�n
            //    OVRInput.SetControllerVibration(frequency, amplitude, touch);
            //    //�w�莞�ԑҋ@
            //    await Task.Delay(milliseconds);
            //    //�U����~
            //    OVRInput.SetControllerVibration(frequency, 0, touch);
            //});
        }

        /// <summary>
        /// �U���J�n����I���܂ł̃^�X�N�����s����
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">���g��0~1(1�̕����@�ׂȋC������)</param>
        /// <param name="amplitude">�U�ꕝ0~1(0�Œ�~)</param>
        /// <param name="time">�U�����ԁA���2�b�炵��</param>
        private void UniTask_ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            int milliseconds = (int)(Mathf.Clamp(time, 0, 2) * 1000);

            UniTask.Void(async () =>
            {
                try
                {
                    //�U���J�n
                    OVRInput.SetControllerVibration(frequency, amplitude, touch);
                    await UniTask.Delay(milliseconds, cancellationToken: cancellation_token);
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("�U������Player���폜");
                }
                finally
                {
                    //�U����~
                    OVRInput.SetControllerVibration(frequency, 0, touch);
                }
            });
        }
    }

}
