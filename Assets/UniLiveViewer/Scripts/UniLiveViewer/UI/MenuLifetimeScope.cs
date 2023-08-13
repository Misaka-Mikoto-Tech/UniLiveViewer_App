using VContainer;
using VContainer.Unity;
using UniLiveViewer;
using UnityEngine;
using VRM.FirstPersonSample;

public class MenuLifetimeScope : LifetimeScope
{
    [SerializeField] Transform _thumbnailRoot;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<ThumbnailController>(Lifetime.Singleton);

        builder.RegisterComponent<Transform>(_thumbnailRoot);

        builder.RegisterComponentInHierarchy<MenuManager>();
        builder.RegisterComponentInHierarchy<CharacterPage>();
        builder.RegisterComponentInHierarchy<AudioPlaybackPage>();

        builder.RegisterComponentInHierarchy<JumpList>();
        
        builder.RegisterComponentInHierarchy<VRMSwitchController>();
        builder.RegisterComponentInHierarchy<VRMRuntimeLoader_Custom>().As<IVRMLoaderUI>();

        builder.RegisterEntryPoint<VRMPresenter>();
        builder.RegisterEntryPoint<ThumbnailPresenter>();
        builder.RegisterEntryPoint<MainMenuPresenter>();
    }
}
