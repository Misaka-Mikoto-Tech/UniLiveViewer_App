using UniLiveViewer.Menu;
using UniLiveViewer.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuGripperService
    {
        Vector3 _distance;

        readonly Renderer _renderer;
        readonly Transform _transform;
        readonly Transform _anchor;
        readonly Transform _player;
        readonly Transform _menu;

        [Inject]
        MenuGripperService(

            Renderer renderer,

            LifetimeScope lifetimeScope,
            Transform anchor,
            PlayerLifetimeScope playerLifetimeScope,
            MenuLifetimeScope menuLifetimeScope)
        {
            _renderer = renderer;
            _transform = lifetimeScope.transform;
            _anchor = anchor;
            _player = playerLifetimeScope.transform;
            _menu = menuLifetimeScope.transform;
        }

        public void Initialize()
        {
            OnSwitchEnable(true);
        }

        public void OnSwitchEnable(bool isEnable)
        {
            _renderer.enabled = isEnable;

            if (isEnable)
            {
                if (1 < _distance.sqrMagnitude) _distance = _distance.normalized;
                var worldPosition = _player.position + _distance;
                _transform.position = worldPosition;
            }
            else
            {
                _distance = _transform.position - _player.position;
            }
        }

        public void OnLateTick()
        {
            _menu.SetPositionAndRotation(_anchor.position, _anchor.rotation);
        }

    }
}