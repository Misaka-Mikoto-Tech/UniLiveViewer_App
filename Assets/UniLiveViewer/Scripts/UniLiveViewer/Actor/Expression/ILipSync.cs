using System;
using UnityEngine;

namespace UniLiveViewer.Actor.Expression
{
    public interface ILipSync
    {
        public void MorphUpdate();

        public void MorphReset();

        public BindInfo[] GetBindInfo();
    }

    [Serializable]
    public class BindInfo
    {
        public LIPTYPE lipType;
        public Transform node;
        public string keyName;
        [HideInInspector] public SkinnedMeshRenderer skinMesh;
        [HideInInspector] public int keyIndex;
    }
}
