using UniLiveViewer;
using VContainer;

public class MovementRestrictionService
{
    readonly SimpleCapsuleWithStickMovement _simpleCapsuleWithStickMovement;
    readonly HandUIController _handUIController;

    [Inject]
    public MovementRestrictionService(
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
