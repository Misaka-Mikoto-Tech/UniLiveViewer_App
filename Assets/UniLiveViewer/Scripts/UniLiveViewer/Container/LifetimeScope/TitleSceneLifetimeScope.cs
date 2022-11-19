using VContainer;
using VContainer.Unity;
using UniLiveViewer;

public class TitleSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<FileAccessManager>();
        builder.RegisterComponentInHierarchy<TitleScene>();

        builder.RegisterEntryPoint<TitleScenePresenter>();
    }
}
