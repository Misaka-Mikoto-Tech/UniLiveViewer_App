using UniLiveViewer.Timeline;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class ActorOptionLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<FakeShadowService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FakeShadowPresenter>();

            builder.Register<GuideAnchorService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GuideAnchorPresenter>();
        }
    }
}

