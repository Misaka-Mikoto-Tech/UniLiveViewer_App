using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;

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
        [SerializeField] JumpList _jumpList;
        [SerializeField] VRMSwitchController _vrmSwitchController;
        [SerializeField] AudioSourceService _audioSourceService;

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<VRMMenuShowMessage>(options);

            builder.RegisterEntryPoint<GraphicsSettingsMenuPresenter>();

            builder.RegisterComponent(_vrmSwitchController);
            builder.RegisterComponent(_audioSourceService);

            ActorPageConfigure(builder);

            builder.RegisterComponent(GetComponent<MenuManager>());

            builder.RegisterComponent<MeneRoot>(_menuRoot);
            builder.RegisterComponent(_audioPlaybackPage);
            builder.RegisterComponent(_itemPage);
            builder.RegisterComponent(_configPage);
            builder.RegisterEntryPoint<MainMenuPresenter>();
        }

        /// <summary>
        /// 理想はページごとにLS分けたい
        /// </summary>
        /// <param name="builder"></param>
        void ActorPageConfigure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_characterPage);
            builder.RegisterComponent(_jumpList);
            builder.Register<ActorEntityFactory>(Lifetime.Singleton);
            builder.Register<ActorRegisterService>(Lifetime.Singleton);
            builder.Register<ActorEntityManagerService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ActorPresenter>();
        }
    }
}
