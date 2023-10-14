using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class StageLifetimeScope : LifetimeScope
    {
        [SerializeField] BlackoutCurtain _blackoutCurtain;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_blackoutCurtain);

            builder.RegisterEntryPoint<StagePresenter>();
        }
    }
}
