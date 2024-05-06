using UniLiveViewer.Player.HandMenu;
using VContainer;

namespace UniLiveViewer.Player.State
{
    public class NonMovableState : IState
    {
        PlayerStateMachineService _stateMachineService;
        readonly SimpleCapsuleWithStickMovement _simpleCapsuleWithStickMovement;
        readonly CameraHeightService _cameraHeightService;
        readonly ActorManipulateService _actorManipulateService;
        readonly ItemMaterialSelectionService _itemMaterialSelection;

        [Inject]
        public NonMovableState(
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
            _simpleCapsuleWithStickMovement.EnableRotation = false;
            _simpleCapsuleWithStickMovement.EnableLinearMovement = false;
        }

        void IState.Update()
        {
            if (!_cameraHeightService.IsShowAny()
                && !_actorManipulateService.IsShowAny()
                && !_itemMaterialSelection.IsShowAny())
            {
                _stateMachineService.ChangeState(PlayerState.Movable);
            }
        }

        void IState.Exit()
        {
            
        }
    }
}
