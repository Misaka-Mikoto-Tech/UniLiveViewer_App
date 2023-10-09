using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UniRx;
using UniLiveViewer.Timeline;

namespace UniLiveViewer
{
    public class StageCharaObserver : MonoBehaviour
    {
        [SerializeField] protected HumanBodyBones target_humanBodyBone = HumanBodyBones.Spine;
        protected Transform[] targets;
        protected List<Transform> targetList;
        protected TimelineController _timeline;

        protected virtual void OnEnable()
        {
            Init();
        }

        // Start is called before the first frame update
        protected virtual void Init()
        {
            if (!_timeline)
            {
                var container = LifetimeScope.Find<TimelineLifetimeScope>().Container;
                _timeline = container.Resolve<TimelineController>();

                _timeline.FieldCharacterCount
                    .Subscribe(_ => Init())
                    .AddTo(this);
            }

            targets = new Transform[SystemInfo.MaxFieldChara];
            targetList = new List<Transform>();
            for (int i = 0; i < SystemInfo.MaxFieldChara; i++)
            {
                var portalChara = _timeline.BindCharaMap[i + 1];
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
