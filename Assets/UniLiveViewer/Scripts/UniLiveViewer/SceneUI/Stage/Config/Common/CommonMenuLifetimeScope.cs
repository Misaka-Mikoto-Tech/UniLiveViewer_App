using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] CommonMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<CommonMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CommonMenuPresenter>();
        }
    }
}
