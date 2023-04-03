using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public interface IMaterialConverter
    {
        UniTask Conversion(CharaController charaCon, CancellationToken token);
        UniTask Conversion_Item(MeshRenderer[] meshRenderers, CancellationToken token);
    }
}