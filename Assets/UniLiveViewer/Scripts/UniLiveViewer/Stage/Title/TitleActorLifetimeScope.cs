using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace UniLiveViewer.Stage.Title.Actor
{
    public class TitleActorLifetimeScope : LifetimeScope
    {
        [SerializeField] TitleActorAnimatorService _titleActorLookatService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_titleActorLookatService);

            builder.RegisterEntryPoint<TitleActorPresenter>();
        }
    }
}
