using System.Collections.Generic;
using UniLiveViewer.OVRCustom;
using UnityEngine;

namespace UniLiveViewer
{
    public class DecorationItemInfo : MonoBehaviour
    {
        public string[] ItemName => itemName;
        [SerializeField] string[] itemName = new string[2] { "アイテム名", "ItemName" };

        public RenderInfo[] RenderInfo => renderInfo;
        [SerializeField] RenderInfo[] renderInfo = new RenderInfo[0];

        [SerializeField] string[] flavorText = new string[2] { "何の変哲もないアイテム", "Unremarkable item" };//未使用
        OVRGrabbable_Custom ovrGrab;
        public bool isAttached;

        /// <summary>
        /// 指定テクスチャに変更
        /// </summary>
        public void SetTexture(int renderInfoIndex,int textureCurrent)
        {
            int i = renderInfoIndex;
            int matIndex = renderInfo[i].data.materialIndex;
            renderInfo[i].data.textureCurrent = textureCurrent;

            foreach (var renderer in renderInfo[i]._renderers)
            {
                foreach (var shaderName in renderInfo[i].data.targetShaderName )
                {
                    renderer.materials[matIndex].SetTexture(
                        shaderName, 
                        renderInfo[i].data.chooseableTexture[renderInfo[i].data.textureCurrent]);
                }
            }
        }

        public bool TryAttachment()
        {
            if (!ovrGrab) ovrGrab = GetComponent<OVRGrabbable_Custom>();
            if(!ovrGrab || !ovrGrab.hitCollider) return false;
            //アタッチする
            ovrGrab.transform.parent = ovrGrab.hitCollider.transform;
            transform.GetComponent<MeshRenderer>().enabled = false;
            isAttached = true;
            return true;
        }

        void OnDestroy()
        {
            if (isAttached) return;

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