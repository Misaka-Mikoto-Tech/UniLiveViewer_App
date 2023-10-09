using UniLiveViewer.Kari;
using UniLiveViewer.OVRCustom;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    public class PlayerLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent<Camera>(Camera.main);

            builder.Register<LocomotionRestrictionService>(Lifetime.Singleton);

            builder.RegisterComponentInHierarchy<OVRManager>();
            builder.RegisterComponentInHierarchy<CharacterCameraConstraint_Custom>();
            builder.RegisterComponentInHierarchy<SimpleCapsuleWithStickMovement>();

            builder.RegisterComponentInHierarchy<VRMTouchColliders>();
            builder.RegisterComponentInHierarchy<PlayerStateManager>();
            builder.RegisterComponentInHierarchy<HandUIController>();
            builder.RegisterComponentInHierarchy<PassthroughService>();

            builder.RegisterEntryPoint<OculusSamplePresenter>();
        }
    }

}