using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class VRMMenuRootService
    {
        readonly VRMMenuRootAnchor _vrmMenuRootAnchor;

        [Inject]
        public VRMMenuRootService(VRMMenuRootAnchor vrmMenuRootAnchor)
        {
            _vrmMenuRootAnchor = vrmMenuRootAnchor;
        }

        public void SetEnableRoot(bool isEnabel)
        {
            if (_vrmMenuRootAnchor.gameObject.activeSelf == isEnabel) return;
            _vrmMenuRootAnchor.gameObject.SetActive(isEnabel);
        }
    }
}