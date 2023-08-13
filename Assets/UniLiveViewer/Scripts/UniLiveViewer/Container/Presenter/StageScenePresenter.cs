using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer;
using UniRx;
using VContainer;
using VContainer.Unity;

public class StageScenePresenter : IAsyncStartable, IDisposable
{
    readonly FileAccessManager _fileAccessManager;
    readonly AnimationAssetManager _animationAssetManager;
    readonly TextureAssetManager _textureAssetManager;
    readonly DirectUI _directUI;
    readonly BlackoutCurtain _blackoutCurtain;
    readonly GeneratorPortal _generatorPortal;
    readonly TimelineController _timelineController;

    readonly CompositeDisposable _disposables;

    [Inject]
    public StageScenePresenter( FileAccessManager fileAccessManager,
                                AnimationAssetManager animationAssetManager,
                                TextureAssetManager textureAssetManager,
                                DirectUI directUI,
                                BlackoutCurtain blackoutCurtain,
                                GeneratorPortal generatorPortal,
                                TimelineController timelineController)
    {
        _fileAccessManager = fileAccessManager;
        _animationAssetManager = animationAssetManager;
        _textureAssetManager = textureAssetManager;
        _timelineController = timelineController;

        _directUI = directUI;
        _blackoutCurtain = blackoutCurtain;
        _generatorPortal = generatorPortal;

        _disposables = new CompositeDisposable();
    }

    async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
    {
        UnityEngine.Debug.Log("Trace: StageScenePresenter.StartAsync");

        //ロード開始
        _fileAccessManager.LoadStartAsObservable
            .Subscribe(_ => _blackoutCurtain.Staging())
            .AddTo(_disposables);

        //ロードエラー
        _fileAccessManager.LoadErrorAsObservable
            .Subscribe(_ => _blackoutCurtain.ShowErrorMessage())
            .AddTo(_disposables);

        //ロード完了
        _fileAccessManager.LoadEndAsObservable
            .Subscribe(_ => OnLoadEnd())
            .AddTo(_disposables);

        _timelineController.FieldCharacterCount
                .Subscribe(_generatorPortal.OnUpdateCharacterCount).AddTo(_disposables);

        _generatorPortal.OnStart(_timelineController, _animationAssetManager);

        //フォルダとファイルの作成
        await _fileAccessManager.OnStartAsync(_animationAssetManager, _textureAssetManager, cancellation);

        UnityEngine.Debug.Log("Trace: StageScenePresenter.StartAsync");
    }

    void OnLoadEnd()
    {
        _blackoutCurtain.Ending().Forget();
        _directUI.Initialize();
        _generatorPortal.OnLoadEnd();
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
