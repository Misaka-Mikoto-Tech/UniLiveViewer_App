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

        readonly Transform _parent;

        [Inject]
        public GuideAnchorService(LifetimeScope lifetimeScope)
        {
            _parent = lifetimeScope.transform;
        }

        public void Setup()
        {
            if (_anchor != null) return;
            _anchor = GameObject.Instantiate(Resources.Load<GameObject>(Path));
            _anchor.transform.parent = _parent;
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

            _anchor.transform.position = _actorEntity.GetAnimator.transform.position;
            var direction = _actorEntity.BoneMap[HumanBodyBones.Head].position - _anchor.transform.position;
            _anchor.transform.forward = direction;
        }
    }
}
