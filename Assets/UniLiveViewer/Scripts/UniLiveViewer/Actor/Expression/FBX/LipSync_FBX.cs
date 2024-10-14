
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniLiveViewer.Actor.Expression
{
    public class LipSync_FBX : MonoBehaviour, ILipSync
    {
        [SerializeField] SkinnedMeshRenderer _skinMesh;
        AnimationCurve _gainCurve;
        const int BLENDSHAPE_WEIGHT = 100;

        [SerializeField] BindInfo[] _bindInfo;

        string[] ILipSync.GetKeyArray() => _customMap.Keys?.ToArray();
        Dictionary<string, int> _customMap = new()
        {
            { "あ", 0 },
            { "い", 0 },
            { "う", 0 },
            { "え", 0 },
            { "お", 0 },
        };


        void Start()
        {
            //シェイプキー名で紐づけ
            for (int i = 0; i < _skinMesh.sharedMesh.blendShapeCount; i++)
            {
                var name = _skinMesh.sharedMesh.GetBlendShapeName(i);
                var target = _bindInfo.FirstOrDefault(x => x.keyName == name);
                if (target == null) continue;
                target.skinMesh = _skinMesh;
                target.keyIndex = i;
            }

            //TODO: また見直す
            foreach (var info in _bindInfo)
            {
                switch (info.lipType)
                {
                    case LIPTYPE.A:
                        _customMap["あ"] = info.keyIndex;
                        break;
                    case LIPTYPE.I:
                        _customMap["い"] = info.keyIndex;
                        break;
                    case LIPTYPE.U:
                        _customMap["う"] = info.keyIndex;
                        break;
                    case LIPTYPE.E:
                        _customMap["え"] = info.keyIndex;
                        break;
                    case LIPTYPE.O:
                        _customMap["お"] = info.keyIndex;
                        break;
                }
            }
        }

        /// <param name="blendShape">使わない</param>
        /// /// <param name="expression">使わない</param>
        void ILipSync.Setup(Transform parent, VRM.VRMBlendShapeProxy blendShape, UniVRM10.Vrm10RuntimeExpression expression)
        {
            if (blendShape != null || expression != null) return;
            transform.SetParent(parent);
            transform.name = ActorConstants.LipSyncController;
        }

        void ILipSync.SetGainCurve(AnimationCurve gainCurve)
        {
            _gainCurve = gainCurve;
        }

        void ILipSync.Morph()
        {
            var total = 1.0f;
            var w = 0.0f;
            foreach (var info in _bindInfo)
            {
                w = total * GetWeight(info.node);
                _skinMesh.SetBlendShapeWeight(info.keyIndex, w * BLENDSHAPE_WEIGHT);
                total -= w;
            }
        }

        void ILipSync.Morph(string key, float weight)
        {
            var index = _customMap[key];
            _skinMesh.SetBlendShapeWeight(index, weight * BLENDSHAPE_WEIGHT);
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void ILipSync.MorphReset()
        {
            foreach (var info in _bindInfo)
            {
                _skinMesh.SetBlendShapeWeight(info.keyIndex, 0);
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
            return _gainCurve.Evaluate(tr.localPosition.z);
        }
    }
}