using System;
using UnityEngine;

namespace UniLiveViewer
{
    // AudioSourceが消せれば
    public class QuasiShadowService : MonoBehaviour
    {
        const string TEXTURE_NAME = "_MainTex";
        QuasiShadowSetting _setting;

        TimelineController _timeline;
        ShadowData[] _shadowDatas;

        RaycastHit _hitCollider;
        Collider[] _hitCollider_L = new Collider[5], hitCollider_R = new Collider[5];

        [Space(10), Header("サウンド")]
        [SerializeField] AudioSource[] audioSource = new AudioSource[5];

        public void OnStart(TimelineController timeline, QuasiShadowSetting quasiShadowSetting)
        {
            _timeline = timeline;
            _setting = quasiShadowSetting;

            GameObject anchor = new GameObject("Shadows");

            //メッシュ消え対策
            var prefab = Instantiate<MeshRenderer>(_setting.MeshRendererPrefab);
            var meshFilter = prefab.GetComponent<MeshFilter>();
            var bounds = meshFilter.mesh.bounds;
            bounds.Expand(100);
            meshFilter.mesh.bounds = bounds;

            _shadowDatas = new ShadowData[_timeline.BindCharaMap.Count];
            for (int i = 0; i < _shadowDatas.Length; i++)
            {
                _shadowDatas[i] = new ShadowData();
                _shadowDatas[i].Init(prefab, anchor.transform);
                _shadowDatas[i].SetMeshRenderers(false, null, null);
            }

            Destroy(prefab.gameObject);

            UpdateShadowType(StageSettingService.UserProfile.CharaShadowType);
        }

        void UpdateShadowType(int shadowType)
        {
            _setting.ShadowType = (SHADOWTYPE)shadowType;
            Update_MeshRenderers();
        }

        public void OnFieldCharacterCount()
        {
            for (int i = 0; i < _shadowDatas.Length; i++)
            {
                if (i == TimelineController.PORTAL_INDEX) continue;
                var chara = _timeline.BindCharaMap[i];
                if (!chara) _shadowDatas[i].SetBodyData(null);
                else _shadowDatas[i].SetBodyData(chara);
            }
            Update_MeshRenderers();
        }

        void Update_MeshRenderers()
        {
            bool isEnable;
            int index = (int)_setting.ShadowType;
            for (int i = 0; i < _shadowDatas.Length; i++)
            {
                if (i == TimelineController.PORTAL_INDEX) continue;
                var chara = _timeline.BindCharaMap[i];
                if (chara && _setting.ShadowType != SHADOWTYPE.NONE) isEnable = true;
                else isEnable = false;
                _shadowDatas[i].SetMeshRenderers(isEnable, _setting.Presets[index].texture_Body, _setting.Presets[index].texture_Foot);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_setting.ShadowType == SHADOWTYPE.NONE) return;
            int index = (int)_setting.ShadowType;
            for (int i = 0; i < _shadowDatas.Length; i++)
            {
                if (_shadowDatas[i].charaCon)
                {
                    Transforming(_shadowDatas[i].charaCon, _shadowDatas[i].spine, _shadowDatas[i].meshRenderer_c, _setting.Presets[index].scala_Body);
                    Transforming(_shadowDatas[i].charaCon, _shadowDatas[i].leftFoot, _shadowDatas[i].meshRenderer_l, _setting.Presets[index].scala_Foot);
                    Transforming(_shadowDatas[i].charaCon, _shadowDatas[i].rightFoot, _shadowDatas[i].meshRenderer_r, _setting.Presets[index].scala_Foot);
                }
            }

            if (SceneManagerService.Current.Mode == SceneMode.GYMNASIUM && _setting.IsStepSE) FootSound();
        }

        void FootSound()
        {
            for (int i = 0; i < _shadowDatas.Length; i++)
            {
                if (!_shadowDatas[i].charaCon) continue;
                //床に向かってrayを飛ばす
                Physics.Raycast(_shadowDatas[i].leftFoot.position, Vector3.down, out _hitCollider, _setting.FootRay, Constants.LayerMaskStageFloor);
                if (_hitCollider.collider != _hitCollider_L[i])
                {
                    _hitCollider_L[i] = _hitCollider.collider;
                    if (_hitCollider_L[i])
                    {
                        audioSource[i].transform.position = _shadowDatas[i].leftFoot.position;
                        audioSource[i].PlayOneShot(_setting.Sounds[UnityEngine.Random.Range(0, _setting.Sounds.Length)]);
                    }
                }
                Physics.Raycast(_shadowDatas[i].rightFoot.position, Vector3.down, out _hitCollider, _setting.FootRay, Constants.LayerMaskStageFloor);
                if (_hitCollider.collider != hitCollider_R[i])
                {
                    hitCollider_R[i] = _hitCollider.collider;
                    if (hitCollider_R[i])
                    {
                        audioSource[i].transform.position = _shadowDatas[i].rightFoot.position;
                        audioSource[i].PlayOneShot(_setting.Sounds[UnityEngine.Random.Range(0, _setting.Sounds.Length)]);
                    }
                }
            }
        }

        void Transforming(CharaController charaCon, Transform targetBone, MeshRenderer targetMesh, float presetScale)
        {
            float scale = presetScale * _setting.ShadowScale * charaCon.CustomScalar;
            Vector3 offset = targetBone.position;
            offset.y = charaCon.transform.position.y;
            float distance = (targetBone.position.y - charaCon.transform.position.y) / charaCon.CustomScalar;

            targetMesh.material.SetVector("_Position", offset);
            targetMesh.material.SetFloat("_Scale", scale * (1 - (distance * 0.4f)));
            targetMesh.material.SetFloat("_Alpha", 1 - (distance * 0.5f));
        }

        void OnDestroy()
        {
            for (int i = 0; i < _shadowDatas.Length; i++)
            {
                _shadowDatas[i].Dispose();
                _shadowDatas[i] = null;
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

            public void Init(MeshRenderer prefab, Transform parent)
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
