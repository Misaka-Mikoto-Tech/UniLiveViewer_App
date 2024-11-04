using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.SpringBone
{
    public class SpringBonePresenter : IStartable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly SpringBoneService _springBoneService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SpringBonePresenter(
            IActorEntity actorEntity,
            SpringBoneService springBoneService)
        {
            _actorEntity = actorEntity;
            _springBoneService = springBoneService;
        }

        void IStartable.Start()
        {
            _actorEntity.ActorEntity()
                .Subscribe(_springBoneService.OnChangeActorEntity).AddTo(_disposables);

            _actorEntity.RawRootScalar()
                .Subscribe(_springBoneService.OnOnChangeActorScale).AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
