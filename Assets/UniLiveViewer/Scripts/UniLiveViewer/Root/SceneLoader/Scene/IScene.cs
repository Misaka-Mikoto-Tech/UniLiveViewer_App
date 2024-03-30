using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniLiveViewer.SceneLoader
{
    //あまり良くないがUIが悪い
    public interface IScene
    {
        UniTask BeginAsync(CancellationToken token);
        string GetVisualName();
    }
}
