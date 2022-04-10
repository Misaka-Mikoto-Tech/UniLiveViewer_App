using UnityEngine;
using System.Collections.Generic;
using VRM;
namespace UniLiveViewer
{
    //CRSを流用、プリセットキャラは改善の余地あり
    public class LipSyncController : MonoBehaviour
    {
        public enum LIPTYPE
        {
            A = 0,
            I,
            U,
            E,
            O
        }

        public class UnityChan_Morphs
        {
            public LIPTYPE lipType;
            public SkinnedMeshRenderer skinMesh;
            public int index;
            public string name;
        }

        [SerializeField] private SkinnedMeshRenderer skinMesh;
        public VRMBlendShapeProxy vrmBlendShape;

        [Header("＜ノード＞")]
        public Transform nodeA;
        public Transform nodeE;
        public Transform nodeI;
        public Transform nodeO;
        public Transform nodeU;

        public AnimationCurve weightCurve;

        [Header("＜紐づけるキー＞")]
        [SerializeField] private string[] linkShapeKey_A;
        [SerializeField] private string[] linkShapeKey_I;
        [SerializeField] private string[] linkShapeKey_U;
        [SerializeField] private string[] linkShapeKey_E;
        [SerializeField] private string[] linkShapeKey_O;

        public CharaController charaCon;

        public Dictionary<string, LIPTYPE> dicUniMorph = new Dictionary<string, LIPTYPE>();
        public List<UnityChan_Morphs> UniMorphs = new List<UnityChan_Morphs>();//使用するモーフ情報
        public Dictionary<BlendShapePreset, LIPTYPE> dicVMRMorph = new Dictionary<BlendShapePreset, LIPTYPE>()
    {
        {BlendShapePreset.A ,LIPTYPE.A},
        {BlendShapePreset.I ,LIPTYPE.I},
        {BlendShapePreset.U ,LIPTYPE.U},
        {BlendShapePreset.E ,LIPTYPE.E},
        {BlendShapePreset.O ,LIPTYPE.O}
    };

        public static void Instantiate(GameObject prefab, CharaController charaController, VRMBlendShapeProxy proxy)
        {

            prefab.GetComponent<LipSyncController>().charaCon = charaController;

            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<LipSyncController>();
            instance.name = "LipSyncController";//(clone)文字列があるとPathが違って困る
            charaController.lipSync = instance;
            instance.transform.parent = charaController.transform;
            instance.vrmBlendShape = proxy;
        }

        void Awake()
        {

        }

        void Start()
        {
            if (!charaCon) charaCon = transform.parent.GetComponent<CharaController>();

            //Dictionary<string, LIPTYPE> dic = new Dictionary<string, LIPTYPE>();

            //VRMはVRMBlendShapeProxyの仕組みを使う
            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                //VRMBlendShapeProxyの割り当てを利用するので特になし
            }
            //その他はこちらで登録
            else
            {
                //対応する組み合わせを登録
                foreach (var keyName in linkShapeKey_A)
                {
                    dicUniMorph.Add(keyName, LIPTYPE.A);
                }
                foreach (var keyName in linkShapeKey_I)
                {
                    dicUniMorph.Add(keyName, LIPTYPE.I);
                }
                foreach (var keyName in linkShapeKey_U)
                {
                    dicUniMorph.Add(keyName, LIPTYPE.U);
                }
                foreach (var keyName in linkShapeKey_E)
                {
                    dicUniMorph.Add(keyName, LIPTYPE.E);
                }
                foreach (var keyName in linkShapeKey_O)
                {
                    dicUniMorph.Add(keyName, LIPTYPE.O);
                }

                for (int i = 0; i < skinMesh.sharedMesh.blendShapeCount; i++)
                {
                    //シェイプキー名を取得
                    string name = skinMesh.sharedMesh.GetBlendShapeName(i);
                    //照合
                    if (dicUniMorph.ContainsKey(key: name))
                    {
                        var e = new UnityChan_Morphs();
                        e.lipType = dicUniMorph[name];
                        e.skinMesh = skinMesh;
                        e.index = i;
                        e.name = name;

                        UniMorphs.Add(e);
                    }
                }
            }
        }

        float GetWeight(Transform tr)
        {
            return weightCurve.Evaluate(tr.localPosition.z);
        }

        void LateUpdate()
        {
            //VMDなら処理しない
            if (charaCon.animationMode == CharaController.ANIMATIONMODE.VMD) return;

            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                var total = 1.0f;

                var w = total * GetWeight(nodeA);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.A), w);
                total -= w;

                w = total * GetWeight(nodeI);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.I), w);
                total -= w;

                w = total * GetWeight(nodeU);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.U), w);
                total -= w;

                w = total * GetWeight(nodeE);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.E), w);
                total -= w;

                w = total * GetWeight(nodeO);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.O), w);
            }
            else
            {
                var total = 100.0f;
                float w = 0;
                foreach (var morph in UniMorphs)
                {
                    switch (morph.lipType)
                    {
                        case LIPTYPE.A:
                            w = total * GetWeight(nodeA);
                            break;
                        case LIPTYPE.I:
                            w = total * GetWeight(nodeI);
                            break;
                        case LIPTYPE.U:
                            w = total * GetWeight(nodeU);
                            break;
                        case LIPTYPE.E:
                            w = total * GetWeight(nodeE);
                            break;
                        case LIPTYPE.O:
                            w = total * GetWeight(nodeO);
                            break;
                    }

                    morph.skinMesh.SetBlendShapeWeight(morph.index, w);
                    total -= w;
                }
            }
        }

        /// <summary>
        /// シェエイプキーを全て初期化する
        /// </summary>
        public void AllClear_BlendShape()
        {
            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
            {
                foreach (var e in UniMorphs)
                {
                    e.skinMesh.SetBlendShapeWeight(e.index, 0);
                }
            }
            else if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                foreach (var e in dicVMRMorph.Keys)
                {
                    vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(e), 0);
                }
            }
        }
    }
}