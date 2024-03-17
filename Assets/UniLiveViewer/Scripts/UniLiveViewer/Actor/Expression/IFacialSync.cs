using System;
using UnityEngine;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public interface IFacialSync
    {
        //TODO: 抽象化できてない
        void Setup(Transform parent, VRMBlendShapeProxy blendShape = null);

        string[] GetKeyArray();

        /// <summary>
        /// Clip用
        /// </summary>
        void Morph();

        /// <summary>
        /// VMD用
        /// </summary>
        void Morph(string key, float weight);

        void MorphReset();

        SkinBindInfo[] GetSkinBindInfo();
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
