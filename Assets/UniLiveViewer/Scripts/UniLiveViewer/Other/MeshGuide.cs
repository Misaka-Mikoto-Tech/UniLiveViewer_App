using UnityEngine;

namespace UniLiveViewer
{
    //�b��I
    public class MeshGuide : MonoBehaviour
    {
        private bool isGuide = false;
        private LookAtController lookAtController;
        private Vector3 distance = Vector3.zero;
        public GameObject guidePrefab;
        private Transform guideObj;
        private MeshRenderer guideMesh;


        // Start is called before the first frame update
        void Start()
        {
            lookAtController = GetComponent<LookAtController>();
            guideObj = Instantiate(guidePrefab).transform;
            Vector3 pos = guideObj.localPosition;
            Vector3 scala = guideObj.localScale;
            guideObj.parent = transform;
            guideObj.localPosition = pos;
            guideMesh = guideObj.gameObject.GetComponent<MeshRenderer>();
            guideObj.localScale = scala;//�T�C�Y��߂�
            guideMesh.enabled = false;//��\���ɂ��Ă���
        }

        // Update is called once per frame
        void Update()
        {
            if (!isGuide) return;
            distance = lookAtController.virtualHead.position - transform.position;
            guideObj.forward = distance;
        }

        public void SetGuide(bool b)
        {
            isGuide = b;
            guideMesh.enabled = isGuide;
        }
    }
}