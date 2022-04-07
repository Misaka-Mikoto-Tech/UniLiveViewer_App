using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UniLiveViewer
{
    public class Button_Base : MonoBehaviour
    {
        [SerializeField] protected bool _isEnable = true;
        public bool isEnable
        {
            get
            {
                return _isEnable;
            }
            set
            {
                _isEnable = value;
                if (collisionChecker)
                {
                    //�����I�ɐe�̏�Ԃɑ�����
                    if (_isEnable) collisionChecker.myState = SWITCHSTATE.ON;
                    else collisionChecker.myState = SWITCHSTATE.OFF;
                }
            }
        }
        //�D���ȕ��g��
        public event Action<Button_Base> onTrigger;
        public UnityEvent onTrigger_Event;//GUI��(�p�t�H�[�}���X�ƃg���[�h�I�t)

        //public bool isTrigger { get;  protected set; }
        [SerializeField] protected float delayTime = 1.0f;

        //Parameter
        public CollisionChecker collisionChecker = null;
        [SerializeField] private Transform neutralAnchor = null;

        protected Rigidbody myRb;
        protected BoxCollider myCol;

        // Start is called before the first frame update
        protected virtual void Awake()
        {
            myRb = collisionChecker.GetComponent<Rigidbody>();
            myCol = collisionChecker.GetComponent<BoxCollider>();
        }

        private void FixedUpdate()
        {
            //�o�l�^������
            AddSpringForce(1500);
            AddSpringForceExtra();
        }

        private void LateUpdate()
        {
            if (myRb.isKinematic) return;
            //�{�^���̈ړ��͈͐���
            if (collisionChecker.transform.localPosition.z >= 0.0f)
            {
                //�{�^���ɐG��Ă����
                if (collisionChecker.Touching())
                {
                    ClickAction();
                }
            }
        }

        /// <summary>
        /// �N���b�N����
        /// </summary>
        public void ClickAction()
        {
            //isTrigger = true;
            if (!gameObject.activeSelf) return;

            StartCoroutine(ClickDirecting());
            //�������Ɣ��肷��
            onTrigger?.Invoke(this);
            onTrigger_Event?.Invoke();
        }

        /// <summary>
        /// �N���b�N���o
        /// </summary>
        protected virtual IEnumerator ClickDirecting()
        {
            //���x�������Ȃ��悤�ɕ������������
            myRb.isKinematic = true;
            myCol.enabled = false;

            //�t���O����������
            isEnable = true;

            //���W������
            collisionChecker.transform.localPosition = Vector3.zero;

            //�U������
            if (collisionChecker.isTouchL) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 1f, 0.1f);

            //��������̃C���^�[�o��
            yield return new WaitForSeconds(delayTime);

            //����t���Ȃ��悤�ɏ����O�i
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;

            //�������Z���ĊJ���A�G�����悤�ɖ߂� 
            myRb.isKinematic = false;
            myCol.enabled = true;
        }

        /// <summary>
        /// �e�L�X�g���b�V���Ɏw�蕶�����ݒ�
        /// </summary>
        /// <param name="str"></param>
        public void SetTextMesh(string str)
        {
            if (collisionChecker.colorSetting == null || !collisionChecker.colorSetting[0].textMesh) return;
            collisionChecker.colorSetting[0].textMesh.text = str;
        }

        /// <summary>
        /// �o�l�̗͂�������
        /// </summary>
        /// <param �o�l�W��="r"></param>
        private void AddSpringForce(float r)
        {
            var diff = neutralAnchor.position - collisionChecker.transform.position; //�o�l�̐L��
            var force = diff * r;
            myRb.AddForce(force);
        }

        /// <summary>
        /// �I�[�o�[�V���[�g���Ȃ��o�l�̗͂�������
        /// </summary>
        private void AddSpringForceExtra()
        {
            var r = myRb.mass * myRb.drag * myRb.drag / 4f;//�o�l�W��
            var diff = neutralAnchor.position - collisionChecker.transform.position; //�o�l�̐L��
            var force = diff * r;
            myRb.AddForce(force);
        }

        private void OnEnable()
        {
            StartCoroutine(InitDirecting());
        }

        private IEnumerator InitDirecting()
        {
            //�����Ȃ��悤�ɕ������������
            myRb.isKinematic = true;
            myCol.enabled = false;

            //���W������
            collisionChecker.transform.localPosition = Vector3.zero;

            //�C���^�[�o��
            yield return new WaitForSeconds(delayTime);

            //����t���Ȃ��悤�ɏ����O�i
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;

            //�������Z���ĊJ���A�G�����悤�ɖ߂� 
            myRb.isKinematic = false;
            myCol.enabled = true;
        }
    }
}