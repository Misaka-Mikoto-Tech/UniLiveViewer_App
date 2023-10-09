using VContainer;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// ロコモーション操作入力を制御する
    /// </summary>
    public class LocomotionRestrictionService
    {
        readonly SimpleCapsuleWithStickMovement _simpleCapsuleWithStickMovement;
        readonly HandUIController _handUIController;

        [Inject]
        public LocomotionRestrictionService(
                SimpleCapsuleWithStickMovement simpleCapsuleWithStickMovement,
                HandUIController handUIController)
        {
            _simpleCapsuleWithStickMovement = simpleCapsuleWithStickMovement;
            _handUIController = handUIController;
        }

        public void MovementRestrictions()
        {
            var isEnable = !_handUIController.IsShow_HandUI();
            _simpleCapsuleWithStickMovement.EnableRotation = isEnable;
            _simpleCapsuleWithStickMovement.EnableLinearMovement = isEnable;
        }
    }
}
