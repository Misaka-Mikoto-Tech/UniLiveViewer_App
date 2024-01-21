﻿using UniLiveViewer.OVRCustom;
using UnityEngine;

namespace UniLiveViewer
{
    [RequireComponent(typeof(OVRGrabbable_Custom))]
    public class DecorationItemInfo : MonoBehaviour
    {
        public string[] ItemName => itemName;
        [SerializeField] string[] itemName = new string[2] { "アイテム名", "ItemName" };

        public RenderInfo[] RenderInfo => renderInfo;
        [SerializeField] RenderInfo[] renderInfo = new RenderInfo[0];

        [SerializeField] string[] flavorText = new string[2] { "何の変哲もないアイテム", "Unremarkable item" };//未使用
        OVRGrabbable_Custom _ovrGrabbableCustom;

        MeshRenderer _meshRenderer;
        bool _isAttached;


        void Awake()
        {
            _ovrGrabbableCustom = GetComponent<OVRGrabbable_Custom>();
            _meshRenderer = transform.GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// 指定テクスチャに変更
        /// </summary>
        public void SetTexture(int renderInfoIndex, int textureCurrent)
        {
            int i = renderInfoIndex;
            int matIndex = renderInfo[i].data.materialIndex;
            renderInfo[i].data.textureCurrent = textureCurrent;

            foreach (var renderer in renderInfo[i]._renderers)
            {
                foreach (var shaderName in renderInfo[i].data.targetShaderName)
                {
                    renderer.materials[matIndex].SetTexture(
                        shaderName,
                        renderInfo[i].data.chooseableTexture[renderInfo[i].data.textureCurrent]);
                }
            }
        }

        public void OnGrabbed(Transform parent)
        {
            transform.parent = parent;
            _meshRenderer.enabled = true;
            _isAttached = false;
        }

        /// <summary>
        /// TODO: これをここでやってるのもそもそも変だがLS化しないと厳しい
        /// </summary>
        /// <returns></returns>
        public bool TryAttachment()
        {
            var collider = _ovrGrabbableCustom.HitCollider;
            if (!collider) return false;
            //アタッチする
            _ovrGrabbableCustom.transform.parent = collider.transform;
            _meshRenderer.enabled = false;
            _isAttached = true;
            return true;
        }

        void OnDestroy()
        {
            if (_isAttached) return;

            for (int i = 0; i < renderInfo.Length; i++)
            {
                for (int j = 0; j < renderInfo[i]._renderers.Length; j++)
                {
                    for (int k = 0; k < renderInfo[i]._renderers[j].materials.Length; k++)
                    {
                        if (!renderInfo[i]._renderers[j].materials[k]) continue;
                        Destroy(renderInfo[i]._renderers[j].materials[k]);
                    }
                }
            }
            renderInfo = null;
        }
    }

    [System.Serializable]
    public class RenderInfo
    {
        public RenderInfoData data;
        public Renderer[] _renderers;
    }
}