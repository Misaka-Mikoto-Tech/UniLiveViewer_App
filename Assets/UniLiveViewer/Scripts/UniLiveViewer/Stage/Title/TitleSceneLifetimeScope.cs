using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace UniLiveViewer.Stage.Title
{
    public class TitleSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] TitleSceneSettings _titleSceneSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_titleSceneSettings);

            builder.RegisterEntryPoint<TitleScenePresenter>();
        }
    }
}
