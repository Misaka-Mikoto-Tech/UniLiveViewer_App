using System.Collections.Generic;
using VRM;

namespace UniLiveViewer
{
    public class LipSync_VRM : LipSyncBase
    {
        public VRMBlendShapeProxy vrmBlendShape;

        public Dictionary<LIPTYPE, BlendShapePreset> dicVMRMorph = new Dictionary<LIPTYPE, BlendShapePreset>()
        {
            {LIPTYPE.A ,BlendShapePreset.A},
            {LIPTYPE.I ,BlendShapePreset.I},
            {LIPTYPE.U ,BlendShapePreset.U},
            {LIPTYPE.E ,BlendShapePreset.E},
            {LIPTYPE.O ,BlendShapePreset.O}
        };

        protected override void Start()
        {
            //VRMBlendShapeProxyの割り当てを利用するので特になし
        }

        public override void MorphReset()
        {
            foreach (var e in dicVMRMorph.Values)
            {
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(e), 0);
            }
        }

        protected override void LateUpdate()
        {
            Morph();
        }

        void Morph()
        {
            var total = 1.0f;
            float w = 0;
            foreach (var e in _bindInfo)
            {
                w = total * GetWeight(e.node);
                var preset = dicVMRMorph[e.lipType];
                var blendShapeKey = BlendShapeKey.CreateFromPreset(preset);
                vrmBlendShape.ImmediatelySetValue(blendShapeKey, w);
                total -= w;
            }
        }
    }
}