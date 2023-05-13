using UnityEngine;

namespace UniLiveViewer
{
    //暫定的
    public class MeshGuide : MonoBehaviour
    {
        [SerializeField] private GameObject guidePrefab;
        bool isShow = false;
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
                        if (i == TimelineController.PORTAL_INDEX) continue;
                        if (!_timelineInfo.GetCharacter(i)) pair[i].guideMesh.enabled = false;
                        else pair[i].guideMesh.enabled = isShow; 
                    }
                }
            } 
        }
        Pair[] pair;
        Vector3 distance = Vector3.zero;
        TimelineController _timeline;
        TimelineInfo _timelineInfo;

        // Start is called before the first frame update
        void Start()
        {
            _timeline = GetComponent<TimelineController>();
            _timelineInfo = _timeline.GetComponent<TimelineInfo>();

            GameObject anchor = new GameObject("GuideMeshs");

            if (_timeline)
            {
                _timeline.FieldCharaAdded += Update_BodyData;
                _timeline.FieldCharaDeleted += Update_BodyData;

                pair = new Pair[_timelineInfo.CharacterCount];
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
                if (i == TimelineController.PORTAL_INDEX) continue;
                var chara = _timelineInfo.GetCharacter(i);
                if (!chara)
                {
                    pair[i].charaController = null;
                    pair[i].head = null;
                    pair[i].guideMesh.enabled = false;
                }
                else
                {
                    pair[i].charaController = chara;
                    pair[i].head = chara.GetAnimator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!isShow) return;

            for (int i = 0; i < pair.Length; i++)
            {
                if (i == TimelineController.PORTAL_INDEX) continue;
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