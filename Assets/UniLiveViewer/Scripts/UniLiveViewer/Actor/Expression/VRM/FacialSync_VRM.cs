using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public class FacialSync_VRM : MonoBehaviour, IFacialSync
    {
        public VRMBlendShapeProxy BlendShapeProxy => _blendShapeProxy;
        [SerializeField] VRMBlendShapeProxy _blendShapeProxy;
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

        public void Setup(Transform parent, VRMBlendShapeProxy blendShape)
        {
            _blendShapeProxy = blendShape;
            transform.SetParent(parent);
            transform.name = ActorConstants.FaceSyncController;
        }

        void IFacialSync.MorphUpdate()
        {
            if (_blendShapeProxy == null) return;

            var total = 1.0f;
            var w = 0.0f;

            // 0許して...
            foreach (var e in _skinBindInfo[0].bindInfo)
            {
                w = total * GetWeight(e.node);
                var preset = dicVMRMorph[e.facialType];
                _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), w);
                total -= w;
            }
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void IFacialSync.MorphReset()
        {
            if (_blendShapeProxy == null) return;

            foreach (var e in dicVMRMorph.Values)
            {
                _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(e), 0);
            }
        }


        /// <summary>
        /// モーフのバインド情報を返す
        /// </summary>
        SkinBindInfo[] IFacialSync.GetSkinBindInfo()
        {
            return _skinBindInfo;
        }

        float GetWeight(Transform tr)
        {
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }
}
