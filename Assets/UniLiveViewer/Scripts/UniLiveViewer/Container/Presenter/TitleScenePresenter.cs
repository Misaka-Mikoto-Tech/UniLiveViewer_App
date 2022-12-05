using System;
using UniLiveViewer;
using VContainer;
using VContainer.Unity;
using UniRx;

public class TitleScenePresenter : IStartable, IDisposable
{
    readonly FileAccessManager _fileAccessManager;
    readonly TitleScene _titleScene;

    readonly CompositeDisposable _disposable;

    [Inject]
    public TitleScenePresenter( FileAccessManager fileAccessManager,
                                TitleScene titleScene)
    {
        _fileAccessManager = fileAccessManager;

        _titleScene = titleScene;

        _disposable = new CompositeDisposable();
    }

    async void IStartable.Start()
    {
        //フォルダとファイルの作成
        await _fileAccessManager.Initialize(null, null);
        _titleScene.Initialize();
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }
}
