using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class StageCharaObserver : MonoBehaviour
    {
        [SerializeField] protected HumanBodyBones target_humanBodyBone = HumanBodyBones.Spine;
        protected Transform[] targets;
        protected List<Transform> targetList;
        protected TimelineController timeline;

        protected virtual void OnEnable()
        {
            Init();
        }

        // Start is called before the first frame update
        protected virtual void Init()
        {
            if (!timeline)
            {
                timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
                timeline.FieldCharaAdded += Init;
                timeline.FieldCharaDeleted += Init;
            }

            targets = new Transform[timeline.maxFieldChara];
            targetList = new List<Transform>();
            for (int i = 0; i < timeline.maxFieldChara; i++)
            {
                if (!timeline.trackBindChara[i + 1]) targets[i] = null;
                else
                {
                    targets[i] = timeline.trackBindChara[i + 1].GetAnimator.GetBoneTransform(target_humanBodyBone);
                    targetList.Add(targets[i]);
                }
            }
        }
    }
}
