using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.SceneLoader
{
    public enum SceneType
    {
        TITLE,
        CANDY_LIVE,
        KAGURA_LIVE,
        VIEWER,
        GYMNASIUM,
    }

    public class SceneChangeService
    {
        static IScene _current;
        OVRScreenFade _screenFade;

        readonly Dictionary<string, IScene> _map;

        public SceneChangeService()
        {
            _map = new Dictionary<string, IScene>
            {
                { "TitleScene", new TitleScene() },
                { "LiveScene", new CandyLiveScene() },
                { "KAGURAScene", new KaguraLiveScene() },
                { "ViewerScene", new ViewerScene() },
                { "GymnasiumScene", new GymnasiumScene() }
            };
        }

        /// <summary>
        /// ちょい変則的なので渡してもらう必要がある
        /// 他シーンではboxなのでパネルはTitleでしか使ってない
        /// </summary>
        /// <param name="screenFade"></param>
        public void Setup(OVRScreenFade screenFade)
        {
            _screenFade = screenFade;
        }

        public async UniTask Change(string nextSceneName, CancellationToken token)
        {
            var scene = _map[nextSceneName];
            await InternalChange(scene, token);
        }

        async UniTask InternalChange(IScene nextScene, CancellationToken token)
        {
            _screenFade?.FadeOut();
            await nextScene.BeginAsync(token);
            _current = nextScene;
            SystemInfo.CheckMaxFieldChara(_current.GetSceneType());
            Debug.Log($"CurrentScene:{_current.GetVisualName()}");
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
            Debug.Log($"CurrentScene:{scene.GetVisualName()}");
        }

        public static string GetVisualName => _current.GetVisualName();

        public static SceneType GetSceneType => _current.GetSceneType();
    }
}
