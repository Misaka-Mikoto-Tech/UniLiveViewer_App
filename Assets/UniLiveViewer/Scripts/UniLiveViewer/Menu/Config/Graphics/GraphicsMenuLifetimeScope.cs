using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] AudioSourceService _audioSourceService;
        [SerializeField] GraphicsMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_audioSourceService);
            builder.RegisterInstance(_settings);

            builder.Register<GraphicsMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GraphicsMenuPresenter>();
        }
    }
}
