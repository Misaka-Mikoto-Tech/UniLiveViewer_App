using VContainer;
using VContainer.Unity;
using UniLiveViewer;

public class TitleSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<SceneChangeService>(Lifetime.Singleton);
        builder.Register<FileAccessManager>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<TitleScene>();
        builder.RegisterComponentInHierarchy<OVRScreenFade>();

        builder.RegisterEntryPoint<TitleScenePresenter>();
    }
}
