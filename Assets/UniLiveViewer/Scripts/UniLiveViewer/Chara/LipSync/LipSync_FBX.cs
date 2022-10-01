
using System.Linq;
using UnityEngine;

namespace UniLiveViewer
{
    public class LipSync_FBX : LipSyncBase
    {
        [SerializeField] SkinnedMeshRenderer _skinMesh;

        protected override void Start()
        {
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

        public override void MorphReset()
        {
            foreach (var e in _bindInfo)
            {
                _skinMesh.SetBlendShapeWeight(e.keyIndex, 0);
            }
        }

        protected override void LateUpdate()
        {
            Morph();
        }

        void Morph()
        {
            var total = 100.0f;
            float w = 0;
            foreach (var e in _bindInfo)
            {
                w = total * GetWeight(e.node);
                _skinMesh.SetBlendShapeWeight(e.keyIndex, w);
                total -= w;
            }
        }
    }

}