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

        public readonly Dictionary<CharaEnums.LIPTYPE, BlendShapePreset> dicVMRMorph = new Dictionary<CharaEnums.LIPTYPE, BlendShapePreset>()
        {
            {CharaEnums.LIPTYPE.A ,BlendShapePreset.A},
            {CharaEnums.LIPTYPE.I ,BlendShapePreset.I},
            {CharaEnums.LIPTYPE.U ,BlendShapePreset.U},
            {CharaEnums.LIPTYPE.E ,BlendShapePreset.E},
            {CharaEnums.LIPTYPE.O ,BlendShapePreset.O}
        };

        /// <summary>
        /// シェイプキーを更新する
        /// </summary>
        void ILipSync.MorphUpdate()
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
        void ILipSync.MorphReset()
        {
            foreach (var e in dicVMRMorph.Values)
            {
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(e), 0);
            }
        }

        /// <summary>
        /// モーフのバインド情報を返す
        /// </summary>
        BindInfo[] ILipSync.GetBindInfo()
        {
            return _bindInfo;
        }

        float GetWeight(Transform tr)
        {
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }
}