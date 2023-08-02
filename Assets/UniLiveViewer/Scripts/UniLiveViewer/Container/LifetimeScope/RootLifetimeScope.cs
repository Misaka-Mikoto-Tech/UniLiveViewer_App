using UniLiveViewer;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        //builder.Register<TimelineInfo>(Lifetime.Singleton);
    }
}
