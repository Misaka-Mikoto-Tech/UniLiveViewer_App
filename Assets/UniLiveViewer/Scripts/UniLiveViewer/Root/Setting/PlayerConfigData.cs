using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/PlayerConfigData", fileName = "PlayerConfigData")]
    public class PlayerConfigData : ScriptableObject
    {
        public IReadOnlyList<LocationMap> Map => _map;
        [SerializeField] List<LocationMap> _map;

        [System.Serializable]
        public class LocationMap
        {
            public SceneType SceneType;
            public Vector3 InitializePosition;
            public Vector3 InitializeRotation;
        }
    }
}