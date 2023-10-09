using VContainer;
using VContainer.Unity;
using UniLiveViewer;

public class StageLightLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<StageLightManager>();

        builder.RegisterEntryPoint<StageLightPresenter>();
    }
}
