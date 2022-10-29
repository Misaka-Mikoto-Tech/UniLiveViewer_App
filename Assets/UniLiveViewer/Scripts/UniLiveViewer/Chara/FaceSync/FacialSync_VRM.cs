using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace UniLiveViewer
{
    public class FacialSync_VRM : MonoBehaviour, IFacialSync
    {
        public VRMBlendShapeProxy vrmBlendShape;
        [SerializeField] AnimationCurve _weightCurve;

        [Header("<keyName不要>")]
        [SerializeField] SkinBindInfo[] _skinBindInfo;
        

        public readonly Dictionary<FACIALTYPE, BlendShapePreset> dicVMRMorph = new Dictionary<FACIALTYPE, BlendShapePreset>()
        {
            {FACIALTYPE.BLINK,BlendShapePreset.Blink},
            {FACIALTYPE.JOY,BlendShapePreset.Joy},
            {FACIALTYPE.ANGRY,BlendShapePreset.Angry},
            {FACIALTYPE.SORROW,BlendShapePreset.Sorrow},
            {FACIALTYPE.SUP,BlendShapePreset.Neutral},
            {FACIALTYPE.FUN,BlendShapePreset.Fun}
        };

        /// <summary>
        /// シェイプキーを更新する
        /// </summary>
        public void MorphUpdate()
        {
            var total = 1.0f;
            var w = 0.0f;

            // NOTE: 0固定許せ...
            foreach (var e in _skinBindInfo[0].bindInfo)
            {
                w = total * GetWeight(e.node);
                var preset = dicVMRMorph[e.facialType];
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), w);
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
        public SkinBindInfo[] GetSkinBindInfo()
        {
            return _skinBindInfo;
        }

        float GetWeight(Transform tr)
        {
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }
}
