using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
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
        StageSettingService _stageSetting;

        [Inject]
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

        public void SetupFirstScene()
        {
            //if (_current != null) return;
            //_stageSetting.Initialize();
        }

        [Inject]
        public void Construct(StageSettingService stageSetting)
        {
            _stageSetting = stageSetting;
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
            _screenFade.FadeOut();
            await nextScene.BeginAsync(token);
            _current = nextScene;
            //_stageSetting.Initialize();
            SystemInfo.CheckMaxFieldChara(_current.GetSceneType());
        }

        public static string GetVisualName => _current.GetVisualName();

        public static SceneType GetSceneType => _current.GetSceneType();
    }
}
