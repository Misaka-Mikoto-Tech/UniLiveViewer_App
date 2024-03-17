using System.Collections.Generic;
using UnityEngine;
using VRM;
using System.Linq;

namespace UniLiveViewer.Actor.Expression
{
    public class LipSync_VRM : MonoBehaviour, ILipSync
    {
        public VRMBlendShapeProxy BlendShapeProxy => _blendShapeProxy;
        [SerializeField] VRMBlendShapeProxy _blendShapeProxy;
        [SerializeField] AnimationCurve _weightCurve;

        [Header("<keyName不要>")]
        [SerializeField] BindInfo[] _bindInfo;

        readonly Dictionary<LIPTYPE, BlendShapePreset> _presetMap = new()
        {
            { LIPTYPE.A, BlendShapePreset.A },
            { LIPTYPE.I, BlendShapePreset.I },
            { LIPTYPE.U, BlendShapePreset.U },
            { LIPTYPE.E, BlendShapePreset.E },
            { LIPTYPE.O, BlendShapePreset.O }
        };

        string[] ILipSync.GetKeyArray() => _customMap.Keys?.ToArray();
        readonly Dictionary<string, BlendShapePreset> _customMap = new()
        {
            { "あ", BlendShapePreset.A },
            { "い", BlendShapePreset.I },
            { "う", BlendShapePreset.U },
            { "え", BlendShapePreset.E },
            { "お", BlendShapePreset.O },
        };

        void ILipSync.Setup(Transform parent, VRMBlendShapeProxy blendShape)
        {
            _blendShapeProxy = blendShape;
            transform.SetParent(parent);
            transform.name = ActorConstants.LipSyncController;
        }

        void ILipSync.Morph()
        {
            if (_blendShapeProxy == null) return;

            var total = 1.0f;
            var w = 0.0f;
            foreach (var info in _bindInfo)
            {
                w = total * GetWeight(info.node);
                var preset = _presetMap[info.lipType];
                var blendShapeKey = BlendShapeKey.CreateFromPreset(preset);
                _blendShapeProxy.ImmediatelySetValue(blendShapeKey, w);
                total -= w;
            }
        }

        void ILipSync.Morph(string key, float weight)
        {
            var preset = _customMap[key];
            _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), weight);
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void ILipSync.MorphReset()
        {
            if (_blendShapeProxy == null) return;

            foreach (var preset in _presetMap.Values)
            {
                _blendShapeProxy.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), 0);
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