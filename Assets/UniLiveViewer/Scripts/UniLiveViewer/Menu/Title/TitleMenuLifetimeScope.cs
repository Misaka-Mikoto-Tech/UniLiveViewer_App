using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class TitleMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] TextMesh _appVersion;
        [SerializeField] TitleMenuService _titleMenuService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_appVersion);
            builder.RegisterComponent(_titleMenuService);

            builder.RegisterEntryPoint<TitleMenuPresenter>();
        }
    }
}