using VContainer;
using VContainer.Unity;
using UniLiveViewer;
using UnityEngine;
using VRM.FirstPersonSample;

public class MenuLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<MenuManager>();
        builder.RegisterComponentInHierarchy<CharacterPage>();
        builder.RegisterComponentInHierarchy<AudioPlaybackPage>();

        builder.RegisterComponentInHierarchy<JumpList>();
        builder.RegisterComponentInHierarchy<ThumbnailController>();
        builder.RegisterComponentInHierarchy<VRMSwitchController>();
        builder.RegisterComponentInHierarchy<VRMRuntimeLoader_Custom>().As<IVRMLoaderUI>();

        builder.RegisterEntryPoint<VRMPresenter>();
        builder.RegisterEntryPoint<ThumbnailPresenter>();
        builder.RegisterEntryPoint<MainMenuPresenter>();
    }
}
