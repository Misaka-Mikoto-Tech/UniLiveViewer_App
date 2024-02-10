using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class SpringBonePresenter : IStartable, IDisposable
    {
        CharaInfoData _charaInfoData;

        readonly IActorEntity _actorEntity;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SpringBonePresenter(
            IActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
        }

        void IStartable.Start()
        {
            _actorEntity.ActorEntity()
                .Subscribe(OnChangeActorEntity).AddTo(_disposables);
        }

        void OnChangeActorEntity(ActorEntity actorEntity)
        {
            if (actorEntity == null) return;

            //揺れもの接触振動
            _charaInfoData = actorEntity.CharaInfoData;
            foreach (var springBone in actorEntity.SpringBoneList)
            {
                springBone.OnHitLeftHand += OnHitLeftHand;
                springBone.OnHitRightHand += OnHitRightHand;
            }
        }

        void OnHitLeftHand()
        {
            ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, _charaInfoData.power, _charaInfoData.time);
        }

        void OnHitRightHand()
        {
            ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, _charaInfoData.power, _charaInfoData.time);
        }

        // TODO: 生成時とPrefab時に切り替えないといけないみたい、最終的に使うか微妙
        void SetEnabelSpringBones(bool isEnabel)
        {
            //foreach (var e in _springBoneList)
            //{
            //    e.enabled = isEnabel;
            //}
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();

            foreach (var springBone in _actorEntity.ActorEntity().Value.SpringBoneList)
            {
                springBone.OnHitLeftHand -= OnHitLeftHand;
                springBone.OnHitRightHand -= OnHitRightHand;
            }
        }
    }
}
