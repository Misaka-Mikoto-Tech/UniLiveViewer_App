using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;
using VRM;

namespace UniLiveViewer.Actor.Expression
{
    public class FacialSync_VRM10 : MonoBehaviour, IFacialSync
    {
        public Vrm10RuntimeExpression RuntimeExpression => _runtimeExpression;
        [SerializeField] Vrm10RuntimeExpression _runtimeExpression;
        [SerializeField] AnimationCurve _weightCurve;

        [Header("<keyName不要>")]
        [SerializeField] SkinBindInfo[] _skinBindInfo;

        readonly Dictionary<FACIALTYPE, ExpressionPreset> _presetMap = new()
        {
            { FACIALTYPE.BLINK, ExpressionPreset.blink },
            { FACIALTYPE.JOY, ExpressionPreset.happy },
            { FACIALTYPE.ANGRY, ExpressionPreset.angry },
            { FACIALTYPE.SORROW, ExpressionPreset.sad },
            { FACIALTYPE.SUP, ExpressionPreset.oh },
            { FACIALTYPE.FUN, ExpressionPreset.relaxed }
        };

        string[] IFacialSync.GetKeyArray() => _customMap.Keys?.ToArray();
        public IReadOnlyDictionary<string, ExpressionPreset> CustomMap => _customMap;
        readonly Dictionary<string, ExpressionPreset> _customMap = new()
        {
            //{ "ウィンク" ,FacialSyncController.FACIALTYPE.BLINK },    
            { "まばたき", ExpressionPreset.blink },
            { "笑い", ExpressionPreset.happy },
            { "怒り", ExpressionPreset.angry },
            { "困る", ExpressionPreset.sad },
            { "にやり", ExpressionPreset.relaxed },
        };

        /// <param name="blendShape">使わない</param>
        void IFacialSync.Setup(Transform parent, VRMBlendShapeProxy blendShape, Vrm10RuntimeExpression expression)
        {
            if (blendShape != null) return;
            if (expression == null) return;
            _runtimeExpression = expression;
            transform.SetParent(parent);
            transform.name = ActorConstants.FaceSyncController;
        }

        Dictionary<ExpressionKey, float> _map = new();
        void IFacialSync.Morph()
        {
            if (_runtimeExpression == null) return;
            var total = 1.0f;
            var w = 0.0f;
            // 0許して...
            foreach (var info in _skinBindInfo[0].bindInfo)
            {
                w = total * GetWeight(info.node);
                var preset = _presetMap[info.facialType];
                _map[ExpressionKey.CreateFromPreset(preset)] = w;
                total -= w;
            }
            _runtimeExpression.SetWeightsNonAlloc(_map);
        }

        void IFacialSync.Morph(string key, float weight)
        {
            var preset = _customMap[key];
            _runtimeExpression.SetWeight(ExpressionKey.CreateFromPreset(preset), weight);
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void IFacialSync.MorphReset()
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
