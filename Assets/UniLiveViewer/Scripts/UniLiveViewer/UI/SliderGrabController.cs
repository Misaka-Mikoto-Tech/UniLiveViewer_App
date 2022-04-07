using UnityEngine;
using System;

namespace UniLiveViewer 
{
    public class SliderGrabController : MonoBehaviour
    {
        public Transform visibleHandler;
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private Transform[] handMesh = new Transform[2];

        [Header("VisibleHandle�̎q�I�u�W�F�N�g���w��")]
        [SerializeField]
        private OVRGrabbable_Custom unVisibleHandler = null;
        private Vector3 nextHandllocalPos;
        private float handleMaxRangeX = 0;
        private float _value = 0;
        public float maxValuel = 1.0f;//�X���C�_�[�̍ő�l
        public float minStepValuel = 0.1f;//�X���C�_�[�𓮂����Ԋu
                                          //private float StepValue = 0.0f;
        [HideInInspector] public bool isControl = false;//���쒆�t���O
        private Vector3 axis = Vector3.zero;

        [SerializeField] private bool SkipMoveMode = false;
        private float coefficient;
        private bool isLHandGrabbed = false;

        public event Action Controled;
        public event Action UnControled;
        public event Action ValueUpdate;

        /// <summary>
        /// �n���h���Ɏw�肵���I�u�W�F�N�g��͈͓��Ő��䂷��(0�`1)
        /// </summary>
        public float Value
        {
            get { return _value; }
            set
            {
                _value = Mathf.Clamp(value, 0, maxValuel);
                nextHandllocalPos.x = handleMaxRangeX * _value / maxValuel;
                visibleHandler.localPosition = startAnchor.localPosition + nextHandllocalPos;
            }
        }

        private void Awake()
        {
            handleMaxRangeX = endAnchor.localPosition.x - startAnchor.localPosition.x;
            //0�ŃX���C�_�[�̈ʒu������������
            Value = 0;
        }

        private void Start()
        {
            //�n���h���̏�����
            initGrabHand();
            //�W������
            coefficient = maxValuel / minStepValuel / 2;
        }

        /// <summary>
        /// �n���h����͂�ł��Ȃ���Ԃɖ߂�
        /// </summary>
        private void initGrabHand()
        {
            unVisibleHandler.transform.parent = visibleHandler;
            unVisibleHandler.transform.localPosition = Vector3.zero;

            handMesh[0].parent.transform.localRotation = Quaternion.identity;

            //UI�phand���\����
            if (handMesh[0].gameObject.activeSelf) handMesh[0].gameObject.SetActive(false);
            if (handMesh[1].gameObject.activeSelf) handMesh[1].gameObject.SetActive(false);
        }

        void Update()
        {
            //�X���C�_�[�񐧌䒆
            if (!isControl)
            {
                //�n���h�����͂܂ꂽ�琧�䒆�ֈڍs
                if (unVisibleHandler.isGrabbed)
                {
                    unVisibleHandler.transform.parent = null;//�K�{
                    isControl = true;

                    //���ۂ̎���\��
                    var realHand = (OVRGrabber_UniLiveViewer)unVisibleHandler.grabbedBy;
                    realHand.handMeshRoot.gameObject.SetActive(false);

                    //UI�p�̎��\��
                    if (unVisibleHandler.grabbedBy.name.Contains("HandL"))
                    {
                        if ((realHand.transform.right).y <= 0)
                        {
                            handMesh[0].parent.transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, 180));
                        }

                        isLHandGrabbed = true;
                        handMesh[0].gameObject.SetActive(true);
                    }
                    else if (unVisibleHandler.grabbedBy.name.Contains("HandR"))
                    {
                        if ((-realHand.transform.right).y <= 0)
                        {
                            handMesh[1].parent.transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, 180));
                        }


                        isLHandGrabbed = false;
                        handMesh[1].gameObject.SetActive(true);
                    }

                    //����J�n
                    Controled?.Invoke();
                }
            }
            //�X���C�_�[���䒆
            else
            {
                //�������Z�o
                Vector3 dt = unVisibleHandler.transform.position - visibleHandler.position;
                //�O��
                axis = Vector3.Cross(visibleHandler.forward, dt);
                var abs = Mathf.Abs(axis.y);
                //���炩�ɓ���
                if (SkipMoveMode && abs >= 0.08f)
                {
                    Value = _value + (coefficient * axis.y * Time.deltaTime);
                    ValueUpdate?.Invoke();
                    //�R���g���[���[�̐U��
                    if (isLHandGrabbed) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 0.2f, 0.05f);
                    else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 0.2f, 0.05f);
                }
                //min�̐ݒ�l�ō���
                else if (abs >= 0.02f)
                {
                    Value = _value + (Mathf.Sign(axis.y) * minStepValuel);
                    ValueUpdate?.Invoke();
                    //�R���g���[���[�̐U��
                    if (isLHandGrabbed) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 0.4f, 0.05f);
                    else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 0.4f, 0.05f);
                }

                //handle�𗣂�����
                if (!unVisibleHandler.isGrabbed)
                {
                    initGrabHand();
                    isControl = false;
                    //����I��
                    UnControled?.Invoke();
                }
            }
        }

        private void OnEnable()
        {
            initGrabHand();
            isControl = false;
        }

        private void OnDisable()
        {
            //�͂܂�Ă�����������
            if (unVisibleHandler.isGrabbed)
            {
                unVisibleHandler.grabbedBy.ForceRelease(unVisibleHandler);
            }

            initGrabHand();
            isControl = false;
        }
    }
}
