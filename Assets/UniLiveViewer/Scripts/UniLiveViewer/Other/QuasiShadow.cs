using System;
using System.Collections.Generic;
using UnityEngine;
using NanaCiel;

namespace UniLiveViewer
{
    public class QuasiShadow : MonoBehaviour
    {
        public enum SHADOWTYPE
        {
            NONE,
            CIRCLE,
            CROSS,
            NONE_CIRCLE,
            NONE_CROSS,
            CIRCLE_CIRCLE,
            CROSS_CROSS,
            CIRCLE_CROSS,
            CROSS_CIRCLE,
        }
        public SHADOWTYPE ShadowType
        {
            get { return shadowType; }
            set
            {
                shadowType = value;
                if ((int)shadowType >= typeLength) shadowType = 0;
                else if ((int)shadowType < 0) shadowType = (SHADOWTYPE)(typeLength - 1);

                Update_MeshRenderers();
            }
        }
        [SerializeField] private MeshRenderer meshRendererPrefab;
        public float shadowScale = 1.0f;

        [SerializeField] private Preset[] preset;

        [Header("確認用露出(readonly)")]
        [SerializeField] private SHADOWTYPE shadowType = SHADOWTYPE.NONE;

        private const string TEXTURE_NAME = "_MainTex";
        private int typeLength = Enum.GetNames(typeof(SHADOWTYPE)).Length;
        private TimelineController timeline;
        private ShadowData[] shadowDatas;

        // Start is called before the first frame update
        void Start()
        {
            timeline = GetComponent<TimelineController>();
            GameObject anchor = new GameObject("Shadows");

            if (timeline)
            {
                timeline.FieldCharaAdded += Update_BodyData;
                timeline.FieldCharaDeleted += Update_BodyData;

                //メッシュ消え対策
                var prefab = Instantiate(meshRendererPrefab);
                var meshFilter = prefab.GetComponent<MeshFilter>();
                var bounds = meshFilter.mesh.bounds;
                bounds.Expand(100);
                meshFilter.mesh.bounds = bounds;

                shadowDatas = new ShadowData[timeline.trackBindChara.Length];
                for (int i = 0; i < shadowDatas.Length; i++)
                {
                    shadowDatas[i] = new ShadowData();
                    shadowDatas[i].Init(prefab, anchor.transform);
                    shadowDatas[i].SetMeshRenderers(false, null, null);
                }

                Destroy(prefab.gameObject);
            }

            shadowScale = SystemInfo.userProfile.CharaShadow;
            ShadowType = (SHADOWTYPE)SystemInfo.userProfile.CharaShadowType;
        }

        private void Update_BodyData()
        {
            for (int i = 0; i < shadowDatas.Length; i++)
            {
                if (i == TimelineController.PORTAL_ELEMENT) continue;
                if (!timeline.trackBindChara[i]) shadowDatas[i].SetBodyData(null);
                else shadowDatas[i].SetBodyData(timeline.trackBindChara[i]);
            }
            Update_MeshRenderers();
        }

        private void Update_MeshRenderers()
        {
            bool isEnable;
            int index = (int)shadowType;
            for (int i = 0; i < shadowDatas.Length; i++)
            {
                if (i == TimelineController.PORTAL_ELEMENT) continue;
                if (timeline.trackBindChara[i] && shadowType != SHADOWTYPE.NONE) isEnable = true;
                else isEnable = false;
                shadowDatas[i].SetMeshRenderers(isEnable, preset[index].texture_Body, preset[index].texture_Foot);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (shadowType == SHADOWTYPE.NONE) return;
            int index = (int)shadowType;
            for (int i = 0; i < shadowDatas.Length; i++)
            {
                if (shadowDatas[i].charaCon)
                {
                    Transforming(shadowDatas[i].charaCon, shadowDatas[i].spine, shadowDatas[i].meshRenderer_c, preset[index].scala_Body);
                    Transforming(shadowDatas[i].charaCon, shadowDatas[i].leftFoot, shadowDatas[i].meshRenderer_l, preset[index].scala_Foot);
                    Transforming(shadowDatas[i].charaCon, shadowDatas[i].rightFoot, shadowDatas[i].meshRenderer_r, preset[index].scala_Foot);
                }
            }
        }

        private void Transforming(CharaController charaCon, Transform targetBone, MeshRenderer targetMesh, float presetScale)
        {
            float scale = presetScale * shadowScale * charaCon.CustomScalar;
            Vector3 offset = targetBone.position;
            offset.y = charaCon.transform.position.y;
            float distance = (targetBone.position.y - charaCon.transform.position.y) / charaCon.CustomScalar;
            
            targetMesh.material.SetVector("_Position", offset);
            targetMesh.material.SetFloat("_Scale", scale * (1 - (distance * 0.4f)));
            targetMesh.material.SetFloat("_Alpha", 1 - (distance * 0.5f));
        }

        private void OnDestroy()
        {
            for (int i = 0; i < shadowDatas.Length; i++)
            {
                shadowDatas[i].Dispose();
                shadowDatas[i] = null;
            }
        }

        [Serializable]
        public class Preset
        {
            public SHADOWTYPE shadowType;
            public Texture2D texture_Body;
            public Texture2D texture_Foot;
            public float scala_Body;
            public float scala_Foot;
        }

        public class ShadowData
        {
            public CharaController charaCon;
            public Transform spine;
            public Transform leftFoot;
            public Transform rightFoot;

            public MeshRenderer meshRenderer_c = new MeshRenderer();
            public MeshRenderer meshRenderer_l = new MeshRenderer();
            public MeshRenderer meshRenderer_r = new MeshRenderer();

            public void Init(MeshRenderer prefab,Transform parent)
            {
                meshRenderer_c = Instantiate(prefab);
                meshRenderer_l = Instantiate(prefab);
                meshRenderer_r = Instantiate(prefab);

                meshRenderer_c.transform.parent = parent;
                meshRenderer_l.transform.parent = parent;
                meshRenderer_r.transform.parent = parent;
            }

            public void SetBodyData(CharaController _charaCon)
            {
                if (_charaCon)
                {
                    charaCon = _charaCon;
                    spine = charaCon.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
                    leftFoot = charaCon.GetAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    rightFoot = charaCon.GetAnimator.GetBoneTransform(HumanBodyBones.RightFoot);
                }
                else
                {
                    charaCon = null;
                    spine = null;
                    leftFoot = null;
                    rightFoot = null;
                }
            }

            public void SetMeshRenderers(bool isEnable, Texture2D tex_body, Texture2D tex_foot)
            {
                meshRenderer_c.material.SetTexture(TEXTURE_NAME, tex_body);
                meshRenderer_l.material.SetTexture(TEXTURE_NAME, tex_foot);
                meshRenderer_r.material.SetTexture(TEXTURE_NAME, tex_foot);

                meshRenderer_c.enabled = tex_body == null ? false : isEnable;
                meshRenderer_l.enabled = tex_foot == null ? false : isEnable;
                meshRenderer_r.enabled = tex_foot == null ? false : isEnable;
            }

            public void Dispose()
            {
                if (meshRenderer_c)
                {
                    Destroy(meshRenderer_c.material);
                    Destroy(meshRenderer_c.gameObject);
                }
                if (meshRenderer_l)
                {
                    Destroy(meshRenderer_l.material);
                    Destroy(meshRenderer_l.gameObject);
                }
                if (meshRenderer_r)
                {
                    Destroy(meshRenderer_r.material);
                    Destroy(meshRenderer_r.gameObject);
                }
            }
        }
    }
}
