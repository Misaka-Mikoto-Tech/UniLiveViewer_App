using System.Collections.Generic;
using UnityEngine;
using VRM;
using System.Linq;
using UniVRM10;

namespace UniLiveViewer.Actor.Expression
{
    public class LipSync_VRM10 : MonoBehaviour, ILipSync
    {
        public Vrm10RuntimeExpression RuntimeExpression => _runtimeExpression;
        [SerializeField] Vrm10RuntimeExpression _runtimeExpression;
        AnimationCurve _gainCurve;
        readonly Dictionary<ExpressionKey, float> _map = new();

        [Header("<keyName不要>")]
        [SerializeField] BindInfo[] _bindInfo;

        readonly Dictionary<LIPTYPE, ExpressionPreset> _presetMap = new()
        {
            { LIPTYPE.A, ExpressionPreset.aa },
            { LIPTYPE.I, ExpressionPreset.ih },
            { LIPTYPE.U, ExpressionPreset.ou },
            { LIPTYPE.E, ExpressionPreset.ee },
            { LIPTYPE.O, ExpressionPreset.oh }
        };

        string[] ILipSync.GetKeyArray() => _customMap.Keys?.ToArray();
        readonly Dictionary<string, ExpressionPreset> _customMap = new()
        {
            { "あ", ExpressionPreset.aa },
            { "い", ExpressionPreset.ih },
            { "う", ExpressionPreset.ou },
            { "え", ExpressionPreset.ee },
            { "お", ExpressionPreset.oh },
        };

        /// <param name="blendShape">使わない</param>
        void ILipSync.Setup(Transform parent, VRMBlendShapeProxy blendShape, Vrm10RuntimeExpression expression)
        {
            if (blendShape != null) return;
            if (expression == null) return;
            _runtimeExpression = expression;
            transform.SetParent(parent);
            transform.name = ActorConstants.LipSyncController;
        }

        void ILipSync.SetGainCurve(AnimationCurve gainCurve)
        {
            _gainCurve = gainCurve;
        }

        void ILipSync.Morph()
        {
            if (_runtimeExpression == null) return;

            var total = 1.0f;
            var w = 0.0f;
            foreach (var info in _bindInfo)
            {
                // TODO: 問題の場所
                w = total * GetWeight(info.node);
                var preset = _presetMap[info.lipType];
                _map[ExpressionKey.CreateFromPreset(preset)] = w;
                total -= w;
            }
            _runtimeExpression.SetWeightsNonAlloc(_map);
        }

        void ILipSync.Morph(string key, float weight)
        {
            var preset = _customMap[key];
            _runtimeExpression.SetWeight(ExpressionKey.CreateFromPreset(preset), weight);
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void ILipSync.MorphReset()
        {
            if (_runtimeExpression == null) return;

            foreach (var preset in _presetMap.Values)
            {
                _map[ExpressionKey.CreateFromPreset(preset)] = 0;
            }
            _runtimeExpression.SetWeightsNonAlloc(_map);
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
            return _gainCurve.Evaluate(tr.localPosition.z);
        }
    }
}