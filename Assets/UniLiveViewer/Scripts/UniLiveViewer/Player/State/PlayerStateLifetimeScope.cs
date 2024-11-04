using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.State
{
    public class PlayerStateLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MovableState>(Lifetime.Singleton);
            builder.Register<NonMovableState>(Lifetime.Singleton);
            builder.Register<PlayerStateMachineService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerStatePresenter>();
        }
    }
}