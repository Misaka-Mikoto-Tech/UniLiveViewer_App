using System;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class GraphicsSettingsMenuPresenter : IStartable, IDisposable
    {
        readonly ConfigPage _configPage;
        readonly GraphicsSettingsService _graphicsSettingsService;
        
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GraphicsSettingsMenuPresenter(
            ConfigPage configPage,
            GraphicsSettingsService graphicsSettingsService)
        {
            _configPage = configPage;
            _graphicsSettingsService = graphicsSettingsService;
        }

        void IStartable.Start()
        {
            _configPage.AntialiasingMode
                .Subscribe(_graphicsSettingsService.ChangeAntialiasing)
                .AddTo(_disposables);
            _configPage.Bloom
                .Subscribe(_graphicsSettingsService.ChangeBloom)
                .AddTo(_disposables);
            _configPage.Tonemapping
                .Subscribe(_graphicsSettingsService.ChangeTonemapping)
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
