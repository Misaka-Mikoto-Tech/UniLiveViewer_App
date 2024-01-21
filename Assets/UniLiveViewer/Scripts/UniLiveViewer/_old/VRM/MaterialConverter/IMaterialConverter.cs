using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public interface IMaterialConverter
    {
        UniTask Convert(IReadOnlyList<SkinnedMeshRenderer> skinnedMeshRenderers, CancellationToken token);
        UniTask Conversion_Item(MeshRenderer[] meshRenderers, CancellationToken token);
    }
}