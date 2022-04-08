using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{
    public class MoveUI : MonoBehaviour
    {
        [SerializeField] private Transform targetAnchor;
        [SerializeField] private SwitchController switchController;

        //public bool isViewerMode = false;
        private bool isInit = false;

        private void Awake()
        {

        }
        private void OnEnable()
        {
            if (!isInit) return;

            if(!targetAnchor.parent.gameObject.activeSelf) targetAnchor.parent.gameObject.SetActive(true);
            //�^�[�Q�b�g�̈ʒu�ֈړ�
            transform.position = targetAnchor.position;
        }

        private void OnDisable()
        {
            if (!isInit) return;
            if (targetAnchor.parent.gameObject.activeSelf) targetAnchor.parent.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine("init");
        }

        // Update is called once per frame
        void Update()
        {
            //if (!isViewerMode) return;

            //�|�[�Y���Ȃ�ȉ��������Ȃ�
            if (Time.timeScale == 0) return;

            //������̐��ʂɍ��킹��
            transform.position = targetAnchor.position;
            transform.rotation = targetAnchor.rotation;
        }

        private IEnumerator init()
        {
            //�ŏ��ɂƂ߂Ă���(�}�j���A�����[�h)
            yield return new WaitForSeconds(0.1f);

            //�L�����𐶐�����
            switchController.initPage();
            yield return new WaitForSeconds(0.1f);

            //��\���ɂ���
            gameObject.SetActive(false);
            isInit = true;
            yield return null;
        }
    }
}