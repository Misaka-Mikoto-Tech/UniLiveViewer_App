using UnityEngine;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// TODO: 消したい
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] PageController pageController;

        public JumpList jumpList => _jumpList;
        [SerializeField] JumpList _jumpList = null;

        void Start()
        {
            pageController.onSwitchPage += () =>
            {
                if (jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(false);
            };
        }
    }
}