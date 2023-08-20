using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer
{
    public enum SceneMode
    {
        CANDY_LIVE,
        KAGURA_LIVE,
        VIEWER,
        GYMNASIUM,
    }

    public class SceneManagerService
    {
        public static int CurrentIndex => _current;
        public static SceneInfo Current => _sceneInfos[_current];
        static List<SceneInfo> _sceneInfos;
        static int _current;

        SceneManagerService()
        {
            _sceneInfos = new List<SceneInfo>()
            {
                new SceneInfo("LiveScene","★CRS Live★", SceneMode.CANDY_LIVE),
                new SceneInfo("KAGURAScene","★KAGURA Live★", SceneMode.KAGURA_LIVE),
                new SceneInfo("ViewerScene","★ViewerScene★", SceneMode.VIEWER),
                new SceneInfo("GymnasiumScene","★Gymnasium★", SceneMode.GYMNASIUM),
            };
            _current = 0;
        }

        public void Initialize(Scene oldScene, Scene newScene)
        {
            _current = _sceneInfos
                .Select((item, index) => new { Item = item, Index = index })
                .FirstOrDefault(x => x.Item.Name == newScene.name).Index;
            Debug.Log($"現在のシーン: {_sceneInfos[_current].Mode}");
        }

        public class SceneInfo
        {
            public string Name { get; private set; }
            public string VisualName { get; private set; }
            public SceneMode Mode { get; private set; }

            public SceneInfo(string name, string visualName, SceneMode mode)
            {
                Name = name;
                VisualName = visualName;
                Mode = mode;
            }
        }
    }
}
