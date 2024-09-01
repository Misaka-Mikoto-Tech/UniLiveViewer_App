using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class GuideAnchorService
    {
        const string Path = "Prefabs/GuideAnchor/GuideBody";

        GameObject _anchor;
        ActorEntity _actorEntity;

        readonly LifetimeScope _parent;

        [Inject]
        public GuideAnchorService(LifetimeScope lifetimeScope)
        {
            _parent = lifetimeScope;
        }

        public void Setup()
        {
            if (_anchor != null) return;
            _anchor = GameObject.Instantiate(Resources.Load<GameObject>(Path));
            _anchor.transform.parent = _parent.transform;
            _anchor.transform.localPosition = Vector3.zero;
            SetEnable(false);
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
        }

        public void SetEnable(bool isEnable)
        {
            if (_anchor == null || _anchor.activeSelf == isEnable) return;
            _anchor.SetActive(isEnable);
        }

        public void OnTick()
        {
            if (_anchor == null || _anchor.activeSelf == false) return;
            if (_actorEntity == null) return;
            var direction = _actorEntity.BoneMap[HumanBodyBones.Head].position - _anchor.transform.position;
            _anchor.transform.forward = direction;
        }
    }
}
