using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace UniLiveViewer
{
    //CRSを流用、プリセットキャラは改善の余地あり
    public class FacialSyncController : MonoBehaviour
    {
        //扱う表情の種類
        public enum FACIALTYPE
        {
            BLINK = 0,
            JOY,
            ANGRY,
            SORROW,
            SUP,
            FUN
        }
        public class UnityChan_Morphs
        {
            public FACIALTYPE facialType;
            public SkinnedMeshRenderer skinMesh;
            public int index;
            public string name;
        }

        public SkinnedMeshRenderer[] uniSkin = new SkinnedMeshRenderer[2];
        public VRMBlendShapeProxy vrmBlendShape;

        [Header("＜ノード＞")]
        //既存ダンスモーフ用
        public Transform nodeBLINK;
        public Transform nodeJOY;
        public Transform nodeANGRY;
        public Transform nodeSORROW;
        public Transform nodeSUP;
        public Transform nodeFUN;

        public AnimationCurve weightCurve;

        [Header("＜紐づけるキー＞")]
        [SerializeField] private string[] linkShapeKey_BLINK;
        [SerializeField] private string[] linkShapeKey_JOY;
        [SerializeField] private string[] linkShapeKey_ANGRY;
        [SerializeField] private string[] linkShapeKey_SORROW;
        [SerializeField] private string[] linkShapeKey_SUP;
        [SerializeField] private string[] linkShapeKey_FUN;

        public bool isManualControl = false;
        public CharaController charaCon;

        public Dictionary<string, FACIALTYPE> dicUniMorph = new Dictionary<string, FACIALTYPE>();
        public List<UnityChan_Morphs> UniMorphs = new List<UnityChan_Morphs>();//使用するモーフ情報

        public Dictionary<BlendShapePreset, FACIALTYPE> dicVMRMorph = new Dictionary<BlendShapePreset, FACIALTYPE>()
        {
            {BlendShapePreset.Blink ,FACIALTYPE.BLINK},
            {BlendShapePreset.Joy ,FACIALTYPE.JOY},
            {BlendShapePreset.Angry ,FACIALTYPE.ANGRY},
            {BlendShapePreset.Sorrow ,FACIALTYPE.SORROW},
            {BlendShapePreset.Fun ,FACIALTYPE.FUN}
        };

        public static void Instantiate(GameObject prefab, CharaController charaController, VRMBlendShapeProxy proxy)
        {
            prefab.GetComponent<FacialSyncController>().charaCon = charaController;

            var instance = Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<FacialSyncController>();
            instance.name = "FaceSyncController";//(clone)文字列があるとPathが違って困る
            charaController.facialSync = instance;
            instance.transform.parent = charaController.transform;
            instance.vrmBlendShape = proxy;
        }

        private void Awake()
        {

        }

        // Start is called before the first frame update
        void Start()
        {
            if (!charaCon) charaCon = transform.parent.GetComponent<CharaController>();

            //VRMはVRMBlendShapeProxyの仕組みを使う
            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                //VRMBlendShapeProxyの割り当てを利用するので特になし
            }
            //その他はこちらで登録
            else
            {
                //Unityちゃん系のモーフリストを作成
                //対応する組み合わせを登録
                foreach (var keyName in linkShapeKey_BLINK)
                {
                    dicUniMorph.Add(keyName, FACIALTYPE.BLINK);
                }
                foreach (var keyName in linkShapeKey_JOY)
                {
                    dicUniMorph.Add(keyName, FACIALTYPE.JOY);
                }
                foreach (var keyName in linkShapeKey_ANGRY)
                {
                    dicUniMorph.Add(keyName, FACIALTYPE.ANGRY);
                }
                foreach (var keyName in linkShapeKey_SORROW)
                {
                    dicUniMorph.Add(keyName, FACIALTYPE.SORROW);
                }
                foreach (var keyName in linkShapeKey_FUN)
                {
                    dicUniMorph.Add(keyName, FACIALTYPE.FUN);
                }

                foreach (var skin in uniSkin)
                {
                    for (int i = 0; i < skin.sharedMesh.blendShapeCount; i++)
                    {
                        //シェイプキー名を取得
                        string name = skin.sharedMesh.GetBlendShapeName(i);

                        //照合
                        if (dicUniMorph.ContainsKey(key: name))
                        {
                            var e = new UnityChan_Morphs();
                            e.facialType = dicUniMorph[name];
                            e.skinMesh = skin;
                            e.index = i;
                            e.name = name;

                            UniMorphs.Add(e);
                        }
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isManualControl) return;
            //VMDなら処理しない
            if (charaCon.animationMode == CharaController.ANIMATIONMODE.VMD) return;

            //VRM既存ダンスモーフ
            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {

                var val = weightCurve.Evaluate(nodeJOY.localPosition.z);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Joy), val);

                val = weightCurve.Evaluate(nodeSORROW.localPosition.z);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Sorrow), val);

                val = weightCurve.Evaluate(nodeBLINK.localPosition.z);
                vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(BlendShapePreset.Blink), val);
            }
            else
            {
                var val = weightCurve.Evaluate(nodeBLINK.localPosition.z);
                var morphs = UniMorphs.Where(x => x.facialType == FACIALTYPE.BLINK);
                foreach (var morph in morphs)
                {
                    morph.skinMesh.SetBlendShapeWeight(morph.index, val * 100);
                }

                val = weightCurve.Evaluate(nodeJOY.localPosition.z);
                morphs = UniMorphs.Where(x => x.facialType == FACIALTYPE.JOY);
                foreach (var morph in morphs)
                {
                    morph.skinMesh.SetBlendShapeWeight(morph.index, val * 100);
                }

                val = weightCurve.Evaluate(nodeANGRY.localPosition.z);
                morphs = UniMorphs.Where(x => x.facialType == FACIALTYPE.ANGRY);
                foreach (var morph in morphs)
                {
                    morph.skinMesh.SetBlendShapeWeight(morph.index, val * 100);
                }

                val = weightCurve.Evaluate(nodeSORROW.localPosition.z);
                morphs = UniMorphs.Where(x => x.facialType == FACIALTYPE.SORROW);
                foreach (var morph in morphs)
                {
                    morph.skinMesh.SetBlendShapeWeight(morph.index, val * 100);
                }

                val = weightCurve.Evaluate(nodeSUP.localPosition.z);
                morphs = UniMorphs.Where(x => x.facialType == FACIALTYPE.SUP);
                foreach (var morph in morphs)
                {
                    morph.skinMesh.SetBlendShapeWeight(morph.index, val * 100);
                }

                val = weightCurve.Evaluate(nodeFUN.localPosition.z);
                morphs = UniMorphs.Where(x => x.facialType == FACIALTYPE.FUN);
                foreach (var morph in morphs)
                {
                    morph.skinMesh.SetBlendShapeWeight(morph.index, val * 100);
                }
            }
        }

        public void SetBlendShape(BlendShapePreset preset, float val)
        {
            if (!isManualControl) return;
            vrmBlendShape.ImmediatelySetValue(BlendShapeKey.CreateFromPreset(preset), val);
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