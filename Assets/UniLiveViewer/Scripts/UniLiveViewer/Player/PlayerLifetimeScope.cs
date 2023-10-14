using UniLiveViewer.Kari;
using UniLiveViewer.OVRCustom;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    [RequireComponent(typeof(PlayerStateManager))]
    [RequireComponent(typeof(HandUIController))]
    [RequireComponent(typeof(VRMTouchColliders))]
    [RequireComponent(typeof(CharacterCameraConstraint_Custom))]
    [RequireComponent(typeof(SimpleCapsuleWithStickMovement))]
    public class PlayerLifetimeScope : LifetimeScope
    {
        //カメラリグ
        [SerializeField] OVRManager _ovrManager;
        [SerializeField] PassthroughService _passthroughService;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent<Camera>(Camera.main);

            builder.Register<LocomotionRestrictionService>(Lifetime.Singleton);

            builder.RegisterComponent(_ovrManager);
            builder.RegisterComponent(_passthroughService);
            builder.RegisterComponent(GetComponent<PlayerStateManager>());
            builder.RegisterComponent(GetComponent<HandUIController>());
            builder.RegisterComponent(GetComponent<VRMTouchColliders>());
            builder.RegisterComponent(GetComponent<CharacterCameraConstraint_Custom>());
            builder.RegisterComponent(GetComponent<SimpleCapsuleWithStickMovement>());

            builder.RegisterEntryPoint<OculusSamplePresenter>();
        }
    }

}