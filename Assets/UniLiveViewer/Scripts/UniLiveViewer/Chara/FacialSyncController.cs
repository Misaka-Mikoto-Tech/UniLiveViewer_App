using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRM;

namespace UniLiveViewer
{
    public class FacialSyncController : MonoBehaviour
    {
        //�����\��̎��
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

        [Header("���m�[�h��")]
        //�����_���X���[�t�p
        public Transform nodeBLINK;
        public Transform nodeJOY;
        public Transform nodeANGRY;
        public Transform nodeSORROW;
        public Transform nodeSUP;
        public Transform nodeFUN;

        public AnimationCurve weightCurve;

        [Header("���R�Â���L�[��")]
        [SerializeField] private string[] linkShapeKey_BLINK;
        [SerializeField] private string[] linkShapeKey_JOY;
        [SerializeField] private string[] linkShapeKey_ANGRY;
        [SerializeField] private string[] linkShapeKey_SORROW;
        [SerializeField] private string[] linkShapeKey_SUP;
        [SerializeField] private string[] linkShapeKey_FUN;

        public bool isManualControl = false;
        public CharaController charaCon;

        public Dictionary<string, FACIALTYPE> dicUniMorph = new Dictionary<string, FACIALTYPE>();
        public List<UnityChan_Morphs> UniMorphs = new List<UnityChan_Morphs>();//�g�p���郂�[�t���

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
            instance.name = "FaceSyncController";//(clone)�����񂪂����Path������č���
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

            //VRM��VRMBlendShapeProxy�̎d�g�݂��g��
            if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                //VRMBlendShapeProxy�̊��蓖�Ă𗘗p����̂œ��ɂȂ�
            }
            //���̑��͂�����œo�^
            else
            {
                //Unity�����n�̃��[�t���X�g���쐬
                //�Ή�����g�ݍ��킹��o�^
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
                        //�V�F�C�v�L�[�����擾
                        string name = skin.sharedMesh.GetBlendShapeName(i);

                        //�ƍ�
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
            //VMD�Ȃ珈�����Ȃ�
            if (charaCon.animationMode == CharaController.ANIMATIONMODE.VMD) return;

            //VRM�����_���X���[�t
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
        /// �V�F�G�C�v�L�[��S�ď���������
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