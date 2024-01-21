using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class VRMMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] MenuRootAnchor _rootAnchor;
        [SerializeField] ThumbnailAnchor _thumbnailAnchor;


        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_rootAnchor);
            builder.RegisterComponent(_thumbnailAnchor);
            builder.Register<MenuRootService>(Lifetime.Singleton);
            builder.Register<ThumbnailService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<VRMMenuPresenter>();
        }
    }
}
