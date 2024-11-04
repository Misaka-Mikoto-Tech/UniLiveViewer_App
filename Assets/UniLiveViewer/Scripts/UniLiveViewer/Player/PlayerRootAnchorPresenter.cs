using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    public class PlayerRootAnchorPresenter : IStartable
    {
        readonly PlayerRootAnchorService _playerRootAnchorService;

        [Inject]
        public PlayerRootAnchorPresenter(
            PlayerRootAnchorService playerRootAnchorService)
        {
            _playerRootAnchorService = playerRootAnchorService;
        }

        void IStartable.Start()
        {
            _playerRootAnchorService.Initialize();
        }
    }
}