using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuRootService
    {
        bool _isEnable;

        readonly RootMenuAnchor _rootMenuAnchor;
        readonly Camera _camera;

        [Inject]
        public MenuRootService(
            RootMenuAnchor rootMenuAnchor,
            Camera camera)
        {
            _rootMenuAnchor = rootMenuAnchor;
            _camera = camera;
        }

        public void Initialize()
        {
            OnMenuSwitching(false);
        }

        public void OnLoadEnd()
        {
            OnMenuSwitching(true);
        }

        public void OnMenuSwitching()
        {
            OnMenuSwitching(!_isEnable);
        }

        void OnMenuSwitching(bool isEnable)
        {
            _isEnable = isEnable;
            _rootMenuAnchor.gameObject.SetActive(isEnable);

            if (!isEnable) return;
            _rootMenuAnchor.transform.position = _camera.transform.TransformPoint(new Vector3(0, -0.45f, 0.32f));
            _rootMenuAnchor.transform.rotation = _camera.transform.rotation * Quaternion.Euler(new Vector3(20, 0, 0));
        }
    }
}