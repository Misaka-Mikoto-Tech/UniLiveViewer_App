using UniLiveViewer.Stage;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VRM.FirstPersonSample;

namespace UniLiveViewer.Menu
{
    [RequireComponent(typeof(MenuManager))]
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField] MeneRoot _menuRoot;
        [SerializeField] Transform _thumbnailRoot;
        

        [SerializeField] CharacterPage _characterPage;
        [SerializeField] AudioPlaybackPage _audioPlaybackPage;
        [SerializeField] JumpList _jumpList;
        [SerializeField] VRMSwitchController _vrmSwitchController;
        [SerializeField] VRMRuntimeLoader_Custom _vrmRuntimeLoader_Custom;
        [SerializeField] GeneratorPortal _generatorPortal;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ThumbnailController>(Lifetime.Singleton);

            builder.RegisterComponent<MeneRoot>(_menuRoot);
            builder.RegisterComponent<Transform>(_thumbnailRoot);

            builder.RegisterComponent(GetComponent<MenuManager>());

            builder.RegisterComponent(_characterPage);
            builder.RegisterComponent(_audioPlaybackPage);
            builder.RegisterComponent(_jumpList);
            builder.RegisterComponent(_vrmSwitchController);
            builder.RegisterComponent(_vrmRuntimeLoader_Custom).As<IVRMLoaderUI>();
            builder.RegisterComponent(_generatorPortal);

            builder.RegisterEntryPoint<VRMPresenter>();
            builder.RegisterEntryPoint<ThumbnailPresenter>();
            builder.RegisterEntryPoint<MainMenuPresenter>();
        }
    }
}
