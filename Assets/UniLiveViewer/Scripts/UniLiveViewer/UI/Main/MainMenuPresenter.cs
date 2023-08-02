using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using VContainer;
using VContainer.Unity;
using UniRx;

namespace UniLiveViewer
{
    public class MainMenuPresenter : IAsyncStartable, IDisposable
    {
        readonly MenuManager _menuManager;
        readonly CharacterPage _characterPage;
        readonly AudioPlaybackPage _audioPlaybackPage;
        readonly AudioAssetManager _audioAssetManager;
        readonly TimelineController _timelineController;

        readonly VRMSwitchController _vrmSwitchController;

        readonly CompositeDisposable _disposables;

        [Inject]
        public MainMenuPresenter(
            MenuManager menuManager,
            CharacterPage characterPage,
            AudioPlaybackPage audioPlaybackPage,
            AudioAssetManager audioAssetManager,
            VRMSwitchController vrmSwitchController,
            TimelineController timelineController)
        {
            _menuManager = menuManager;
            _characterPage = characterPage;
            _audioPlaybackPage = audioPlaybackPage;
            _audioAssetManager = audioAssetManager;

            _vrmSwitchController = vrmSwitchController;
            _timelineController = timelineController;

            _disposables = new CompositeDisposable();
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("Trace: MainMenuPresenter.Start");

            _timelineController.FieldCharacterCount
                .Subscribe(_characterPage.OnUpdateCharacterCount).AddTo(_disposables);

            await UniTask.Yield(cancellation);//Timelineの初期化を待つ

            _characterPage.Initialize(_menuManager, _vrmSwitchController);
            _audioPlaybackPage.Initialize(_audioAssetManager);

            UnityEngine.Debug.Log("Trace: MainMenuPresenter.Start");
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
