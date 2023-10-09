using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuGripperLifetimeScope : LifetimeScope
    {
        [SerializeField] Renderer _renderer;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(transform);
            builder.RegisterComponent(_renderer);
            builder.Register<MenuGripperService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MenuGripperPresenter>();
        }
    }
}
