using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniLiveViewer.SceneLoader
{
    /// <summary>
    /// 使わない想定
    /// </summary>
    public class TitleScene : IScene
    {
        const int BufferTime = 5000;

        public TitleScene()
        {
        }

        async UniTask IScene.BeginAsync(CancellationToken token)
        {
            // 使わない
        }

        string IScene.GetVisualName() => "TitleScene";
    }
}
