using System.Collections.Generic;
using UnityEngine;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public class LipSync_VRM : MonoBehaviour, ILipSync
    {
        public VRMBlendShapeProxy BlendShapeProxy => _blendShapeProxy;
        [SerializeField] VRMBlendShapeProxy _blendShapeProxy;
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

        public void Setup(Transform parent, VRMBlendShapeProxy blendShape)
        {
            _blendShapeProxy = blendShape;
            transform.SetParent(parent);
            transform.name = ActorConstants.LipSyncController;
        }

        void ILipSync.MorphUpdate()
        {
            if (_blendShapeProxy == null) return;

            var total = 1.0f;
            var w = 0.0f;
            foreach (var e in _bindInfo)
            {
                w = total * GetWeight(e.node);
                var preset = dicVMRMorph[e.lipType];
                var blendShapeKey = BlendShapeKey.CreateFromPreset(preset);
                _blendShapeProxy.ImmediatelySetValue(blendShapeKey, w);
                total -= w;
            }
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void ILipSync.MorphReset()
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