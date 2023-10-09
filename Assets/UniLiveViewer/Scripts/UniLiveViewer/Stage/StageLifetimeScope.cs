using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class StageLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<BlackoutCurtain>();

            builder.RegisterEntryPoint<StagePresenter>();
        }
    }
}
