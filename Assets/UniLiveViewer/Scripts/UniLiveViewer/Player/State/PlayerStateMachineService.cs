using System.Collections.Generic;
using UniRx;
using VContainer;

namespace UniLiveViewer.Player.State
{
    public enum PlayerState
    {
        Movable,
        NonMovable
    }

    /// <summary>
    /// 両手の状態に応じて移動可能かの可否が決まる
    /// </summary>
    public class PlayerStateMachineService
    {
        PlayerState _current;
        ReactiveProperty<IState> _currentState = new();
        readonly Dictionary<PlayerState, IState> _map;

        [Inject]
        public PlayerStateMachineService(
            MovableState movableState,
            NonMovableState nonMovableState)
        {
            //循環依存回避...汚い
            movableState.Setup(this);
            nonMovableState.Setup(this);

            _map = new Dictionary<PlayerState, IState>()
            {
                { PlayerState.Movable,movableState},
                { PlayerState.NonMovable,nonMovableState},
            };

            _current = PlayerState.Movable;
            _currentState.Value = _map[_current];
            _currentState.Value.Enter();
        }

        public void ChangeState(PlayerState newState)
        {
            if (_current == newState) return;

            _currentState.Value.Exit();
            _current = newState;
            _currentState.Value = _map[newState];
            _currentState.Value.Enter();
        }

        public void OnTick()
        {
            _currentState.Value.Update();
        }
    }
}
