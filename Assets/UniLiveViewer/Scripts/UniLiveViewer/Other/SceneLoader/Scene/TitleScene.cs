using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace UniLiveViewer.SceneLoader
{
    public class TitleScene : IScene
    {
        const int BufferTime = 5000;

        public TitleScene()
        {

        }

        async UniTask IScene.BeginAsync(CancellationToken token)
        {
            //完全非同期は無理
            var nextSceneName = FileReadAndWriteUtility.UserProfile.LastSceneName;
            var async = SceneManager.LoadSceneAsync(nextSceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(BufferTime, cancellationToken: token);
            async.allowSceneActivation = true;
        }


        SceneType IScene.GetSceneType() => SceneType.TITLE;

        string IScene.GetVisualName() => "TitleScene";
    }
}
