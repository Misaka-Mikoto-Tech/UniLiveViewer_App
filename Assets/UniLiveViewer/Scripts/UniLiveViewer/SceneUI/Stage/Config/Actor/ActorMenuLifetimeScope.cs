using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] ActorMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<ActorMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ActorMenuPresenter>();
        }
    }
}
