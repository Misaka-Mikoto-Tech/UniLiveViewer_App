using VContainer;
using VContainer.Unity;
using UniLiveViewer;
using VRM.FirstPersonSample;

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

        builder.RegisterComponentInHierarchy<VRMSwitchController>();
        builder.RegisterComponentInHierarchy<VRMRuntimeLoader_Custom>().As<IVRMLoaderUI>();

        builder.RegisterEntryPoint<VRMPresenter>();
        builder.RegisterEntryPoint<StageScenePresenter>();
    }
}
