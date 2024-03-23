using System;
using UnityEngine;
using UniVRM10;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public interface ILipSync
    {
        //TODO: 抽象化できてない
        void Setup(Transform parent, VRMBlendShapeProxy blendShape = null, Vrm10RuntimeExpression expression = null);

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

        BindInfo[] GetBindInfo();
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
