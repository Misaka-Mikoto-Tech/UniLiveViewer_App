using UniLiveViewer.Menu;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuGripperService
    {
        const float LerpSpeed = 25.0f;

        readonly Renderer _renderer;
        readonly Transform _transform;
        readonly Transform _anchor;
        readonly Transform _menu;
        readonly Camera _camera;

        [Inject]
        public MenuGripperService(
            Renderer renderer,
            LifetimeScope lifetimeScope,
            Transform anchor,
            MenuLifetimeScope menuLifetimeScope,
            Camera camera)
        {
            _renderer = renderer;
            _transform = lifetimeScope.transform;
            _anchor = anchor;
            _menu = menuLifetimeScope.transform;
            _camera = camera;
        }

        public void Initialize()
        {
            OnMenuSwitching(true);
        }

        public void OnMenuSwitching(bool isEnable)
        {
            _renderer.enabled = isEnable;

            // anchorに見直す
            if (!isEnable) return;
            var pos = _camera.transform.position + (_camera.transform.forward * 0.425f) + new Vector3(0, -0.4f, 0);
            var rot = _camera.transform.rotation * Quaternion.Euler(new Vector3(20, 0, 0));
            _transform.SetPositionAndRotation(pos, rot);

            _menu.SetPositionAndRotation(pos, rot);
        }

        public void OnLateTick()
        {
            var moveStep = LerpSpeed * Time.deltaTime;
            _menu.SetPositionAndRotation(
                Vector3.Lerp(_menu.position, _anchor.position, moveStep),
                Quaternion.Lerp(_menu.rotation, _anchor.rotation, moveStep));
        }
    }
}