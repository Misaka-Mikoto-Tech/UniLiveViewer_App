using System;
using UnityEngine;

namespace UniLiveViewer
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
        public CharaEnums.LIPTYPE lipType;
        public Transform node;
        public string keyName;
        [HideInInspector] public SkinnedMeshRenderer skinMesh;
        [HideInInspector] public int keyIndex;
    }
}
