using UniLiveViewer.Menu;
using UniLiveViewer.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// MenuLifetimeScopeを直接GripするとSpriteが崩れるので代わりにハンドルになる
    /// </summary>
    public class MenuGripperLifetimeScope : LifetimeScope
    {
        [SerializeField] Transform _menuAnchor;
        [SerializeField] Renderer _renderer;
        [SerializeField] PlayerLifetimeScope _playerLifetimeScope;
        [SerializeField] MenuLifetimeScope _menuLifetimeScope;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_playerLifetimeScope);
            builder.RegisterComponent(_menuAnchor);
            builder.RegisterComponent(_renderer);
            builder.RegisterComponent(_menuLifetimeScope);
            builder.Register<MenuGripperService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<MenuGripperPresenter>();
        }
    }
}
