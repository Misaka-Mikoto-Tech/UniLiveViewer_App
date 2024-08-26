using Cysharp.Threading.Tasks;
using System;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class TitleMenuPresenter : IInitializable, IStartable, IDisposable
    {
        readonly SceneChangeService _sceneChangeService;
        readonly TitleMenuService _menuService;
        readonly TextMesh _textAppVersion;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TitleMenuPresenter(
            TitleMenuService titleMenuService,
            SceneChangeService sceneChangeService,
            TextMesh textAppVersion)
        {
            _sceneChangeService = sceneChangeService;
            _menuService = titleMenuService;
            _textAppVersion = textAppVersion;
        }

        void IInitializable.Initialize()
        {
            _sceneChangeService.Initialize();
        }

        void IStartable.Start()
        {
            _textAppVersion.text = "ver." + Application.version;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}