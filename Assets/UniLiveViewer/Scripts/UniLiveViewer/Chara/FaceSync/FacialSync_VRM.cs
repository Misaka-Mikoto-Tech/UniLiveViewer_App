using System.Collections.Generic;
using VRM;

namespace UniLiveViewer
{
    public class FacialSync_VRM : FacialSyncBase
    {
        public VRMBlendShapeProxy vrmBlendShape;

        public readonly Dictionary<FACIALTYPE, BlendShapePreset> dicVMRMorph = new Dictionary<FACIALTYPE, BlendShapePreset>()
        {
            {FACIALTYPE.BLINK,BlendShapePreset.Blink},
            {FACIALTYPE.JOY,BlendShapePreset.Joy},
            {FACIALTYPE.ANGRY,BlendShapePreset.Angry},
            {FACIALTYPE.SORROW,BlendShapePreset.Sorrow},
            {FACIALTYPE.SUP,BlendShapePreset.Neutral},
            {FACIALTYPE.FUN,BlendShapePreset.Fun}
        };

        // Start is called before the first frame update
        protected override void Start()
        {
            //VRMBlendShapeProxyの割り当てを利用するので特になし
        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        public override void MorphReset()
        {
            foreach (var e in dicVMRMorph.Values)
            {
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(e), 0);
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            Morph();
        }

        void Morph()
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
    }
}
