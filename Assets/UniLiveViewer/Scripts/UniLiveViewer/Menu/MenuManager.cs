using UnityEngine;
using UniRx;

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
            pageController.ChangePageAsObservable
                .Where(_ => jumpList.gameObject.activeSelf)
                .Subscribe(_ => jumpList.gameObject.SetActive(false));
        }
    }
}