using UnityEngine;

namespace UniLiveViewer
{
    public class ItemMaterialSelector : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] Quads = new MeshRenderer[8];//候補とりあえず8
        [SerializeField] private Transform currentQuad;//カーソルの役割
        private Vector3 currentQuadOffset = new Vector3(0, 0, 0.01f);//zファイ対策
        [SerializeField] private TextMesh textMesh;
        [SerializeField] private Animator anime;
        private int current = 0;
        private int limitTex;

        private DecorationItemInfo itemInfo;
        private int languageCode;

        private void Awake()
        {
            languageCode = SystemInfo.userProfile.LanguageCode;
        }

        /// <summary>
        /// アイテム名、候補テクスチャをセット
        /// </summary>
        /// <param name="info"></param>
        public void Init(DecorationItemInfo info)
        {
            itemInfo = info;
            textMesh.text = itemInfo.ItemName[languageCode];

            if (info.RenderInfo.Length == 0)
            {
                current = 0;
                for (int i = 0; i < Quads.Length; i++)
                {
                    if (Quads[i].gameObject.activeSelf) Quads[i].gameObject.SetActive(false);
                }
            }
            else
            {
                current = itemInfo.RenderInfo[0].data.textureCurrent;
                limitTex = itemInfo.RenderInfo[0].data.chooseableTexture.Length;

                for (int i = 0; i < Quads.Length; i++)
                {
                    if (i < limitTex)
                    {
                        if (!Quads[i].gameObject.activeSelf) Quads[i].gameObject.SetActive(true);
                        Quads[i].material.SetTexture("_BaseMap", itemInfo.RenderInfo[0].data.chooseableTexture[i]);
                    }
                    else
                    {
                        if (Quads[i].gameObject.activeSelf) Quads[i].gameObject.SetActive(false);
                    }
                }
            }
            //カーソル移動
            UpdateCursor();
        }

        public bool TrySetTexture(int nextCurrent)
        {
            if (nextCurrent < limitTex && current != nextCurrent)
            {
                current = nextCurrent;
                itemInfo.SetTexture(0, current);//現状は0しかないので固定

                //カーソル移動
                UpdateCursor();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Currentへカーソル画像を移動する
        /// </summary>
        private void UpdateCursor()
        {
            currentQuad.parent = Quads[current].transform;
            currentQuad.transform.localPosition = currentQuadOffset;
            currentQuad.transform.localRotation = Quaternion.identity;
        }
    }
}