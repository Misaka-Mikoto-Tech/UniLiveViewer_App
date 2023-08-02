using System;
using UniLiveViewer;
using VContainer;
using VContainer.Unity;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;

public class TitleScenePresenter : IAsyncStartable, IDisposable
{
    readonly FileAccessManager _fileAccessManager;
    readonly TitleScene _titleScene;
    readonly SceneChangeService _sceneChangeService;

    readonly CompositeDisposable _disposable;

    [Inject]
    public TitleScenePresenter( FileAccessManager fileAccessManager,
                                TitleScene titleScene,
                                SceneChangeService sceneChangeService)
    {
        _fileAccessManager = fileAccessManager;
        _titleScene = titleScene;
        _sceneChangeService = sceneChangeService;

        _disposable = new CompositeDisposable();
    }

    async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
    {
        _titleScene.ChangeSceneAsObservable
            .Subscribe(async x => await _sceneChangeService.Change(x, 5000, cancellation))
            .AddTo(_disposable);

        //フォルダとファイルの作成
        await _fileAccessManager.Initialize(null, null, cancellation);
        _titleScene.Begin();
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
