using Cysharp.Threading.Tasks;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuPresenter : IStartable, IDisposable
    {
        readonly SceneSelectMenuService _sceneSelectMenuService;
        readonly SceneSelectMenuSettings _settings;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public SceneSelectMenuPresenter(
            SceneSelectMenuService sceneSelectMenuService,
            SceneSelectMenuSettings settings)
        {
            _sceneSelectMenuService = sceneSelectMenuService;
            _settings = settings;
        }

        void IStartable.Start()
        {
            _settings.ChangeSceneAsObservable
                .Subscribe(x => _sceneSelectMenuService.OnChangeSceneAsync(x).Forget()).AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }

}