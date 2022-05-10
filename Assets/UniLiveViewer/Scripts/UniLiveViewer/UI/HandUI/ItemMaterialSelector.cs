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

        // Start is called before the first frame update
        void Start()
        {

        }

        /// <summary>
        /// アイテム名、候補テクスチャをセット
        /// </summary>
        /// <param name="info"></param>
        public void Init(DecorationItemInfo info)
        {
            current = 0;
            textMesh.text = info.itemName[(int)SystemInfo.userProfile.data.LanguageCode];

            limitTex = info.texs.Length;

            for (int i = 0; i < Quads.Length; i++)
            {
                if (i < limitTex)
                {
                    if(!Quads[i].gameObject.activeSelf)Quads[i].gameObject.SetActive(true);
                    Quads[i].material.SetTexture("_BaseMap", info.texs[i]);
                }
                else
                {
                    if (Quads[i].gameObject.activeSelf) Quads[i].gameObject.SetActive(false);
                } 
            }

            //カーソル移動
            currentQuad.parent = Quads[current].transform;
            currentQuad.transform.localPosition = currentQuadOffset;
            currentQuad.transform.localRotation = Quaternion.identity;
        }

        public Texture TryGetTexture(int nextCurrent)
        {
            Texture result = null;
            if (nextCurrent < limitTex && current != nextCurrent)
            {
                current = nextCurrent;
                currentQuad.parent = Quads[current].transform;
                currentQuad.transform.localPosition = currentQuadOffset;
                currentQuad.transform.localRotation = Quaternion.identity;

                result = Quads[current].material.GetTexture("_BaseMap");
            }
            return result;
        }
    }
}