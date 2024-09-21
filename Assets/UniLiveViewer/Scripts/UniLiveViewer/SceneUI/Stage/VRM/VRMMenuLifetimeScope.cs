using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class VRMMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] VRMMenuRootAnchor _vrmMenuRootAnchor;
        [SerializeField] ThumbnailAnchor _thumbnailAnchor;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_vrmMenuRootAnchor);
            builder.RegisterComponent(_thumbnailAnchor);
            builder.Register<VRMMenuRootService>(Lifetime.Singleton);
            builder.Register<ThumbnailService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<VRMMenuPresenter>();
        }
    }
}
