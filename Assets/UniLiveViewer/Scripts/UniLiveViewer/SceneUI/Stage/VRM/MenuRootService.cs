using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{

    public class MenuRootService
    {
        readonly MenuRootAnchor _root;

        [Inject]
        public MenuRootService(MenuRootAnchor menuRootAnchor)
        {
            _root = menuRootAnchor;
        }

        public void SetEnableRoot(bool isEnabel)
        {
            if (_root.gameObject.activeSelf == isEnabel) return;
            _root.gameObject.SetActive(isEnabel);
        }
    }
}