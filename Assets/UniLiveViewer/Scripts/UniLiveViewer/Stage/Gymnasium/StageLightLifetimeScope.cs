using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Gymnasium
{
    [RequireComponent(typeof(StageLightChangeService))]
    public class StageLightLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponent<StageLightChangeService>());

            builder.RegisterEntryPoint<StageLightPresenter>();
        }
    }
}
