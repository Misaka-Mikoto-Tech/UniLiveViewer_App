
using System.Linq;
using UnityEngine;

namespace UniLiveViewer
{
    public class LipSync_FBX : MonoBehaviour, ILipSync
    {
        [SerializeField] SkinnedMeshRenderer _skinMesh;
        [SerializeField] AnimationCurve _weightCurve;
        const int BLENDSHAPE_WEIGHT = 100;

        [SerializeField] BindInfo[] _bindInfo;

        void Start()
        {
            transform.name = "LipSyncController";

            //シェイプキー名で紐づけ
            for (int i = 0; i < _skinMesh.sharedMesh.blendShapeCount; i++)
            {
                string name = _skinMesh.sharedMesh.GetBlendShapeName(i);
                BindInfo target = _bindInfo.FirstOrDefault(x => x.keyName == name);
                if (target != null)
                {
                    target.keyIndex = i;
                    target.skinMesh = _skinMesh;
                }
            }
        }

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
                _skinMesh.SetBlendShapeWeight(e.keyIndex, w * BLENDSHAPE_WEIGHT);
                total -= w;
            }
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        void ILipSync.MorphReset()
        {
            foreach (var e in _bindInfo)
            {
                _skinMesh.SetBlendShapeWeight(e.keyIndex, 0);
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