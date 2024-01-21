using System;
using UnityEngine;

namespace UniLiveViewer.Actor.Expression
{
    public interface IFacialSync
    {
        public void MorphUpdate();

        public void MorphReset();

        public SkinBindInfo[] GetSkinBindInfo();
    }

    [Serializable]
    public class SkinBindInfo
    {
        public SkinnedMeshRenderer skinMesh;//FBXだけ
        public BindInfo[] bindInfo;

        [Serializable]
        public class BindInfo
        {
            public FACIALTYPE facialType;
            public Transform node;
            public KeyPair[] keyPair;

            [Serializable]
            public class KeyPair
            {
                public string name;
                [HideInInspector] public int index;
            }
        }
    }
}
