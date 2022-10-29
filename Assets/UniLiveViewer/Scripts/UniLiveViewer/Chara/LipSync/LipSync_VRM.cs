using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace UniLiveViewer
{
    public class LipSync_VRM : MonoBehaviour, ILipSync
    {
        public VRMBlendShapeProxy vrmBlendShape;
        [SerializeField] AnimationCurve _weightCurve;
        
        [Header("<keyName不要>")]
        [SerializeField] BindInfo[] _bindInfo;

        public readonly Dictionary<LIPTYPE, BlendShapePreset> dicVMRMorph = new Dictionary<LIPTYPE, BlendShapePreset>()
        {
            {LIPTYPE.A ,BlendShapePreset.A},
            {LIPTYPE.I ,BlendShapePreset.I},
            {LIPTYPE.U ,BlendShapePreset.U},
            {LIPTYPE.E ,BlendShapePreset.E},
            {LIPTYPE.O ,BlendShapePreset.O}
        };

        /// <summary>
        /// シェイプキーを更新する
        /// </summary>
        public void MorphUpdate()
        {
            var total = 1.0f;
            var w = 0.0f;
            foreach (var e in _bindInfo)
            {
                w = total * GetWeight(e.node);
                var preset = dicVMRMorph[e.lipType];
                var blendShapeKey = BlendShapeKey.CreateFromPreset(preset);
                vrmBlendShape.ImmediatelySetValue(blendShapeKey, w);
                total -= w;
            }
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        public void MorphReset()
        {
            foreach (var e in dicVMRMorph.Values)
            {
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(e), 0);
            }
        }

        /// <summary>
        /// モーフのバインド情報を返す
        /// </summary>
        public BindInfo[] GetBindInfo()
        {
            return _bindInfo;
        }

        float GetWeight(Transform tr)
        {
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }
}