using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class StageCharaObserver : MonoBehaviour
    {
        [SerializeField] protected HumanBodyBones target_humanBodyBone = HumanBodyBones.Spine;
        protected Transform[] targets;
        protected List<Transform> targetList;
        protected TimelineController _timeline;
        protected TimelineInfo _timelineInfo;

        protected virtual void OnEnable()
        {
            Init();
        }

        // Start is called before the first frame update
        protected virtual void Init()
        {
            if (!_timeline)
            {
                _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
                _timelineInfo = _timeline.GetComponent<TimelineInfo>();
                _timeline.FieldCharaAdded += Init;
                _timeline.FieldCharaDeleted += Init;
            }

            targets = new Transform[_timelineInfo.MaxFieldChara];
            targetList = new List<Transform>();
            for (int i = 0; i < _timelineInfo.MaxFieldChara; i++)
            {
                var portalChara = _timelineInfo.GetCharacter(i + 1);
                if (!portalChara) targets[i] = null;
                else
                {
                    targets[i] = portalChara.GetAnimator.GetBoneTransform(target_humanBodyBone);
                    targetList.Add(targets[i]);
                }
            }
        }
    }
}
