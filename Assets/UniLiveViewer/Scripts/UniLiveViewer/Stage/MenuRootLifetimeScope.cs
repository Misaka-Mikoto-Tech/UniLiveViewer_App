using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuRootLifetimeScope : LifetimeScope
    {
        [SerializeField] RootMenuAnchor _meneRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_meneRoot);

            builder.Register<MenuRootService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MenuRootPresenter>();
        }
    }
}
