using UniLiveViewer.Player.HandMenu;
using VContainer;

namespace UniLiveViewer.Player.State
{
    public class MovableState : IState
    {
        PlayerStateMachineService _stateMachineService;
        readonly SimpleCapsuleWithStickMovement _simpleCapsuleWithStickMovement;
        readonly CameraHeightService _cameraHeightService;
        readonly ActorManipulateService _actorManipulateService;
        readonly ItemMaterialSelectionService _itemMaterialSelection;

        [Inject]
        public MovableState(
            SimpleCapsuleWithStickMovement simpleCapsuleWithStickMovement,
            CameraHeightService cameraHeightService,
            ActorManipulateService actorManipulateService,
            ItemMaterialSelectionService itemMaterialSelection)
        {
            _simpleCapsuleWithStickMovement = simpleCapsuleWithStickMovement;
            _cameraHeightService = cameraHeightService;
            _actorManipulateService = actorManipulateService;
            _itemMaterialSelection = itemMaterialSelection;
        }

        public void Setup(PlayerStateMachineService stateMachineService)
        {
            _stateMachineService = stateMachineService;
        }

        void IState.Enter()
        {
            _simpleCapsuleWithStickMovement.EnableRotation = true;
            _simpleCapsuleWithStickMovement.EnableLinearMovement = true;
        }

        void IState.Update()
        {
            if (_cameraHeightService.IsShowAny() 
                || _actorManipulateService.IsShowAny() 
                || _itemMaterialSelection.IsShowAny())
            {
                _stateMachineService.ChangeState(PlayerState.NonMovable);
            }
        }

        void IState.Exit()
        {
            
        }
    }
}
