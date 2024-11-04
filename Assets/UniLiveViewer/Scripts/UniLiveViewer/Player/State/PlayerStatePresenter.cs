using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.State
{
    public class PlayerStatePresenter : ITickable
    {
        readonly PlayerStateMachineService _playerStateMachineService;

        [Inject]
        public PlayerStatePresenter(PlayerStateMachineService playerStateMachineService)
        {
            _playerStateMachineService = playerStateMachineService;
        }

        void ITickable.Tick()
        {
            _playerStateMachineService.OnTick();
        }
    }
}