using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace UniLiveViewer.SceneLoader
{
    public enum SceneType
    {
        TITLE,
        CANDY_LIVE,
        KAGURA_LIVE,
        VIEWER,
        GYMNASIUM,
        FANTASY_VILLAGE,
    }

    public class SceneChangeService
    {
        //雑
        public static string[] NameList = new string[]
        { "LiveScene", "KAGURAScene", "ViewerScene", "GymnasiumScene", "FantasyVillage" };

        static IScene _current;

        readonly Dictionary<string, IScene> _map;

        public SceneChangeService()
        {
            _map = new Dictionary<string, IScene>
            {
                { "TitleScene", new TitleScene() },
                { "LiveScene", new CandyLiveScene() },
                { "KAGURAScene", new KaguraLiveScene() },
                { "ViewerScene", new ViewerScene() },
                { "GymnasiumScene", new GymnasiumScene() },
                { "FantasyVillage", new FantasyVillageScene() }
            };
        }

        public void SetupTitleScene()
        {
            _current = _map["TitleScene"];
        }

        public async UniTask Change(string nextSceneName, CancellationToken token)
        {
            var scene = _map[nextSceneName];
            await InternalChange(scene, token);
        }

        async UniTask InternalChange(IScene nextScene, CancellationToken token)
        {
            await nextScene.BeginAsync(token);
            _current = nextScene;
            SystemInfo.CheckMaxFieldChara(_current.GetSceneType());
        }

        /// <summary>
        /// 直接Sceneから再生するEditor限定
        /// </summary>
        public void SetSceneIfNecessary()
        {
            if (_current != null) return;
            var name = FileReadAndWriteUtility.UserProfile.LastSceneName;
            var scene = _map[name];
            _current = scene;
            SystemInfo.CheckMaxFieldChara(_current.GetSceneType());
        }

        public static string GetVisualName => _current.GetVisualName();

        public static SceneType GetSceneType => _current.GetSceneType();
    }
}
