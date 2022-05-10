using UnityEngine;

namespace UniLiveViewer
{
    //暫定的
    public class MeshGuide : MonoBehaviour
    {
        [SerializeField] private GameObject guidePrefab;
        private bool isShow = false;
        public bool IsShow 
        { 
            get { return isShow; } 
            set 
            { 
                if(isShow != value)
                {
                    isShow = value;

                    for (int i = 0; i < pair.Length; i++)
                    {
                        if (i == TimelineController.PORTAL_ELEMENT) continue;
                        if (!timeline.trackBindChara[i]) pair[i].guideMesh.enabled = false;
                        else pair[i].guideMesh.enabled = isShow; 
                    }
                }
            } 
        }
        private Pair[] pair;
        private Vector3 distance = Vector3.zero;
        private TimelineController timeline;

        // Start is called before the first frame update
        void Start()
        {
            timeline = GetComponent<TimelineController>();

            GameObject anchor = new GameObject("GuideMeshs");

            if (timeline)
            {
                timeline.FieldCharaAdded += Update_BodyData;
                timeline.FieldCharaDeleted += Update_BodyData;

                pair = new Pair[timeline.trackBindChara.Length];
                for (int i = 0; i < pair.Length; i++)
                {
                    pair[i] = new Pair();
                    pair[i].guideMesh = Instantiate(guidePrefab).GetComponent<MeshRenderer>();
                    pair[i].guideMesh.transform.parent = anchor.transform;
                    pair[i].guideMesh.enabled = false;//非表示にしておく
                }
            }
        }
                
        private void Update_BodyData()
        {
            for (int i = 0; i < pair.Length; i++)
            {
                if (i == TimelineController.PORTAL_ELEMENT) continue;
                if (!timeline.trackBindChara[i])
                {
                    pair[i].charaController = null;
                    pair[i].head = null;
                    pair[i].guideMesh.enabled = false;
                }
                else
                {
                    pair[i].charaController = timeline.trackBindChara[i];
                    pair[i].head = timeline.trackBindChara[i].GetAnimator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isShow) return;

            for (int i = 0; i < pair.Length; i++)
            {
                if (i == TimelineController.PORTAL_ELEMENT) continue;
                if (pair[i].charaController)
                {
                    pair[i].guideMesh.transform.position = pair[i].charaController.transform.position;
                    distance = pair[i].head.position - pair[i].guideMesh.transform.position;
                    pair[i].guideMesh.transform.forward = distance;
                }
            }
        }
    }

    public class Pair
    {
        public CharaController charaController;
        public Transform head;
        public MeshRenderer guideMesh;
    }
}