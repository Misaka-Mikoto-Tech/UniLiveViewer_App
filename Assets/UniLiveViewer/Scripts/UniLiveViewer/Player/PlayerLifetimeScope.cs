using System.Collections.Generic;
using UniLiveViewer.OVRCustom;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    [RequireComponent(typeof(PlayerStateManager))]
    [RequireComponent(typeof(VRMTouchColliders))]
    [RequireComponent(typeof(CharacterCameraConstraint_Custom))]
    [RequireComponent(typeof(SimpleCapsuleWithStickMovement))]
    public class PlayerLifetimeScope : LifetimeScope
    {
        [SerializeField] PlayerHandMenuAnchorL _playerHandMenuAnchorL;
        [SerializeField] PlayerHandMenuAnchorR _playerHandMenuAnchorR;

        [SerializeField] PlayerConfigData _playerConfigData;
        [SerializeField] OVRManager _ovrManager;
        [SerializeField] PassthroughService _passthroughService;
        /// <summary>
        /// inspector上のInject設定も必要(他手段もある)
        /// </summary>
        [SerializeField] List<OVRGrabber_UniLiveViewer> _ovrGrabbers = new();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent<Camera>(Camera.main);

            builder.RegisterInstance(_playerConfigData);

            builder.RegisterComponent(_playerHandMenuAnchorL);
            builder.RegisterComponent(_playerHandMenuAnchorR);
            builder.RegisterComponent(_ovrGrabbers);
            builder.RegisterComponent(_ovrManager);
            builder.RegisterComponent(_passthroughService);
            builder.RegisterComponent(GetComponent<PlayerStateManager>());
            builder.RegisterComponent(GetComponent<VRMTouchColliders>());
            builder.RegisterComponent(GetComponent<CharacterCameraConstraint_Custom>());
            builder.RegisterComponent(GetComponent<SimpleCapsuleWithStickMovement>());

            builder.RegisterEntryPoint<OculusSamplePresenter>();
        }
    }
}