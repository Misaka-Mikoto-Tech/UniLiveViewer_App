using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuPresenter : IStartable , IDisposable
    {
        readonly ActorMenuService _actorMenuService;
        readonly ActorMenuSettings _settings;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public ActorMenuPresenter(
            ActorMenuService actorMenuService,
            ActorMenuSettings settings)
        {
            _actorMenuService = actorMenuService;
            _settings = settings;
        }

        void IStartable.Start()
        {
            _settings.InitialActorSizeSlider.EndDriveAsObservable
                .Subscribe(_ => _actorMenuService.OnUnControledActorSize()).AddTo(_disposables);
            _settings.InitialActorSizeSlider.ValueAsObservable
                .Subscribe(_actorMenuService.OnUpdateActorSize).AddTo(_disposables);

            _settings.FallingShadowSlider.EndDriveAsObservable
                .Subscribe(_ => _actorMenuService.OnUnControledFallingShadow()).AddTo(_disposables);
            _settings.FallingShadowSlider.ValueAsObservable
                .Subscribe(_actorMenuService.OnUpdateFallingShadow).AddTo(_disposables);

            _actorMenuService.Initialize();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}