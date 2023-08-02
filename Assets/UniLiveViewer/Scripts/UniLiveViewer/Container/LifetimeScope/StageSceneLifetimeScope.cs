using UniLiveViewer;
using VContainer;
using VContainer.Unity;

public class StageSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<FileAccessManager>(Lifetime.Singleton);
        builder.Register<AnimationAssetManager>(Lifetime.Singleton);
        builder.Register<TextureAssetManager>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<DirectUI>();
        builder.RegisterComponentInHierarchy<BlackoutCurtain>();
        builder.RegisterComponentInHierarchy<GeneratorPortal>();

        builder.RegisterEntryPoint<StageScenePresenter>();
    }
}
