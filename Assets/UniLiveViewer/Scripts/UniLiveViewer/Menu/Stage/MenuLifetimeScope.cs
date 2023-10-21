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
        [Header("各ページ")]
        [SerializeField] CharacterPage _characterPage;
        [SerializeField] AudioPlaybackPage _audioPlaybackPage;
        [SerializeField] ItemPage _itemPage;
        [SerializeField] ConfigPage _configPage;

        [Header("その他")]
        [SerializeField] MeneRoot _menuRoot;
        [SerializeField] GeneratorPortal _generatorPortal;
        [SerializeField] VRMSwitchController _vrmSwitchController;
        [SerializeField] Transform _thumbnailRoot;
        [SerializeField] JumpList _jumpList;
        [SerializeField] VRMRuntimeLoader_Custom _vrmRuntimeLoader_Custom;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ThumbnailController>(Lifetime.Singleton);

            builder.RegisterComponent<MeneRoot>(_menuRoot);
            builder.RegisterComponent<Transform>(_thumbnailRoot);

            builder.RegisterComponent(GetComponent<MenuManager>());

            builder.RegisterComponent(_characterPage);
            builder.RegisterComponent(_audioPlaybackPage);
            builder.RegisterComponent(_itemPage);
            builder.RegisterComponent(_configPage);

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
