using System;
using UniLiveViewer;
using VContainer;
using VContainer.Unity;
using UniRx;
using Cysharp.Threading.Tasks;

public class StageScenePresenter : IStartable, IDisposable
{
    readonly FileAccessManager _fileAccessManager;
    readonly AnimationAssetManager _animationAssetManager;
    readonly AudioAssetManager _audioAssetManager;
    readonly TextureAssetManager _textureAssetManager;
    readonly DirectUI _directUI;
    readonly BlackoutCurtain _blackoutCurtain;
    readonly GeneratorPortal _generatorPortal;

    readonly CompositeDisposable _disposable;

    [Inject]
    StageScenePresenter( FileAccessManager fileAccessManager,
                                AnimationAssetManager animationAssetManager,
                                AudioAssetManager audioAssetManager,
                                TextureAssetManager textureAssetManager,
                                DirectUI directUI,
                                BlackoutCurtain blackoutCurtain,
                                GeneratorPortal generatorPortal)
    {
        _fileAccessManager = fileAccessManager;
        _animationAssetManager = animationAssetManager;
        _audioAssetManager = audioAssetManager;
        _textureAssetManager = textureAssetManager;

        _directUI = directUI;
        _blackoutCurtain = blackoutCurtain;
        _generatorPortal = generatorPortal;

        _disposable = new CompositeDisposable();
    }

    async void IStartable.Start()
    {
        //ロード開始
        _fileAccessManager.LoadStartAsObservable
            .Subscribe(_ => _blackoutCurtain.Staging())
            .AddTo(_disposable);

        //ロードエラー
        _fileAccessManager.LoadErrorAsObservable
            .Subscribe(_ => _blackoutCurtain.ShowErrorMessage())
            .AddTo(_disposable);

        //ロード完了
        _fileAccessManager.LoadEndAsObservable
            .Subscribe(_ => _blackoutCurtain.Ending().Forget())
            .AddTo(_disposable);
        _fileAccessManager.LoadEndAsObservable
            .Subscribe(_ => _directUI.Initialize())
            .AddTo(_disposable);
        _fileAccessManager.LoadEndAsObservable
            .Subscribe(_ => _generatorPortal.Initialize())
            .AddTo(_disposable);

        //フォルダとファイルの作成
        await _fileAccessManager.Initialize(_animationAssetManager, _textureAssetManager);
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
