using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class MainMenuPresenter : IAsyncStartable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;
        readonly TimelineController _timelineController;
        readonly PlayerStateManager _playerStateManager;

        readonly MeneRoot _meneRoot;
        readonly CharacterPage _characterPage;
        readonly AudioPlaybackPage _audioPlaybackPage;
        readonly GeneratorPortal _generatorPortal;

        readonly CompositeDisposable _disposables;

        [Inject]
        public MainMenuPresenter(
            FileAccessManager fileAccessManager,
            TimelineController timelineController,
            PlayerStateManager playerStateManager,
            MeneRoot meneRoot,
            CharacterPage characterPage,
            AudioPlaybackPage audioPlaybackPage,
            GeneratorPortal generatorPortal)
        {
            _fileAccessManager = fileAccessManager;
            _timelineController = timelineController;
            _playerStateManager = playerStateManager;
            _meneRoot = meneRoot;
            _characterPage = characterPage;
            _audioPlaybackPage = audioPlaybackPage;
            _generatorPortal = generatorPortal;

            _disposables = new CompositeDisposable();
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _generatorPortal.OnLoadEnd())
                .AddTo(_disposables);
            _timelineController.FieldCharacterCount
                .Subscribe(OnFieldCharacterCount)
                .AddTo(_disposables);
            _playerStateManager.MainUISwitchingAsObservable
                .Subscribe(SwitchEnable)
                .AddTo(_disposables);


            _characterPage.OnStart();
            _audioPlaybackPage.OnStart();
        }

        void SwitchEnable(bool isEnable)
        {
            if (_meneRoot.gameObject.activeSelf != isEnable) _meneRoot.gameObject.SetActive(isEnable);
        }

        void OnFieldCharacterCount(int i)
        {
            _characterPage.OnUpdateCharacterCount(i);
            _generatorPortal.OnUpdateCharacterCount(i);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
