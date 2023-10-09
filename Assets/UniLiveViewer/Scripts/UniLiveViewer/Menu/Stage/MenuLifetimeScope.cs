using UniLiveViewer.Stage;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VRM.FirstPersonSample;

namespace UniLiveViewer.Menu
{
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField] MeneRoot _menuRoot;
        [SerializeField] Transform _thumbnailRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ThumbnailController>(Lifetime.Singleton);

            builder.RegisterComponent<MeneRoot>(_menuRoot);
            builder.RegisterComponent<Transform>(_thumbnailRoot);

            builder.RegisterComponentInHierarchy<MenuManager>();
            builder.RegisterComponentInHierarchy<CharacterPage>();
            builder.RegisterComponentInHierarchy<AudioPlaybackPage>();

            builder.RegisterComponentInHierarchy<JumpList>();

            builder.RegisterComponentInHierarchy<VRMSwitchController>();
            builder.RegisterComponentInHierarchy<VRMRuntimeLoader_Custom>().As<IVRMLoaderUI>();

            builder.RegisterComponentInHierarchy<GeneratorPortal>();

            builder.RegisterEntryPoint<VRMPresenter>();
            builder.RegisterEntryPoint<ThumbnailPresenter>();
            builder.RegisterEntryPoint<MainMenuPresenter>();
        }
    }
}
