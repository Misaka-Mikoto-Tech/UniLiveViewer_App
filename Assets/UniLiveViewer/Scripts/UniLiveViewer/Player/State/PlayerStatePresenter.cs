using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.State
{
    public class PlayerStatePresenter : IStartable, ITickable
    {
        readonly PlayerStateMachineService _playerStateMachineService;

        [Inject]
        public PlayerStatePresenter(PlayerStateMachineService playerStateMachineService)
        {
            _playerStateMachineService = playerStateMachineService;
        }

        void IStartable.Start()
        {

        }

        void ITickable.Tick()
        {
            _playerStateMachineService.OnTick();
        }
    }
}