using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    [RequireComponent(typeof(PlayerHandVRMCollidersService))]
    public class StageLifetimeScope : LifetimeScope
    {
        [SerializeField] BlackoutCurtain _blackoutCurtain;
        [SerializeField] PlayerHandVRMCollidersService _playerHandVRMCollidersService;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_blackoutCurtain);
            builder.RegisterComponent(_playerHandVRMCollidersService);
            builder.RegisterEntryPoint<StagePresenter>();
        }
    }
}
