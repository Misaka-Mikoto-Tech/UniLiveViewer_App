using System.Linq;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Player
{
    public class PlayerRootAnchorService
    {
        readonly PlayerRootAnchor _playerRootAnchor;
        readonly PlayerConfigData _playerConfigData;

        [Inject]
        public PlayerRootAnchorService(PlayerRootAnchor playerRootAnchor, PlayerConfigData playerConfigData)
        {
            _playerRootAnchor = playerRootAnchor;
            _playerConfigData = playerConfigData;
        }

        public void Initialize()
        {
            var map = _playerConfigData.Map.FirstOrDefault(x => x.SceneType == SceneChangeService.GetSceneType);
            if (map == null)
            {
                //PlayerConfigDataで設定する
                Debug.LogWarning("There are no settings for this world");
                return;
            }
            _playerRootAnchor.transform.SetPositionAndRotation(map.InitializePosition, Quaternion.Euler(map.InitializeRotation));
        }
    }
}