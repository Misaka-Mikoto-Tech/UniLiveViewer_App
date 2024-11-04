using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] TitleMenuSettings _titleMenuSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_titleMenuSettings);

            builder.Register<TitleMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<TitleMenuPresenter>();
        }
    }
}