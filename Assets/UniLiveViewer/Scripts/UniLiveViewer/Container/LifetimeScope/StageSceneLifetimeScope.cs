using VContainer;
using VContainer.Unity;
using UniLiveViewer;

public class StageSceneLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<FileAccessManager>();
        builder.RegisterComponentInHierarchy<AnimationAssetManager>();
        builder.RegisterComponentInHierarchy<AudioAssetManager>();
        builder.RegisterComponentInHierarchy<TextureAssetManager>();

        builder.RegisterComponentInHierarchy<DirectUI>();
        builder.RegisterComponentInHierarchy<BlackoutCurtain>();
        builder.RegisterComponentInHierarchy<GeneratorPortal>();

        builder.RegisterEntryPoint<StageScenePresenter>();
    }
}
