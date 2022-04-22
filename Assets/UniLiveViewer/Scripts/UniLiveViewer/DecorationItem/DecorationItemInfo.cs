using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class DecorationItemInfo : MonoBehaviour
    {
        public string[] itemName = new string[2] { "ItemName", "アイテム名" };
        public string[] flavorText = new string[2] { "Unremarkable item", "何の変哲もないアイテム" };
        public Texture2D[] texs = new Texture2D[0];
        public RenderInfo[] renderInfo = new RenderInfo[1];

        // Start is called before the first frame update
        void Start()
        {
            try
            {
                foreach (var info in renderInfo)
                {
                    for (int i = 0; i < info._renderer.materials.Length; i++)
                    {
                        try
                        {
                            //レンダーとそのマテリアルプロパティを取得
                            //info.materialPropertyBlock.Add(new MaterialPropertyBlock());
                            //info._renderer.GetPropertyBlock(info.materialPropertyBlock[i]);
                            //初期カラーを取得しておく
                            //TODO:ここライトマップのアイテムだとエラー、いずれ直す
                            info.initColor.Add(info._renderer.materials[i].GetColor("_BaseColor"));
                        }
                        catch
                        {
                            Debug.Log("_BaseColorがありません。ライトマップしかない奴");
                        }
                    }

                    //マテリアルオブジェの衝突イベント
                    if (info.itemCollisionChecker) info.itemCollisionChecker.OnTrigger += SetTexture;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 指定テクスチャに変更
        /// </summary>
        public void SetTexture(ItemCollisionChecker parts, Collider other)
        {
            var newTextur = other.GetComponent<MeshRenderer>().materials[0].GetTexture("_BaseMap");

            foreach (var info in renderInfo)
            {
                //部位の特定
                if (info.itemCollisionChecker == parts)
                {
                    for (int i = 0; i < info._renderer.materials.Length; i++)
                    {
                        info._renderer.materials[i].SetTexture("_BaseMap", newTextur);
                    }

                    //複製しないとPrefab編集が面倒になるのでやめた
                    //対象メッシュの全マテリアルのTexを置換
                    //for (int i = 0; i < info.materialPropertyBlock.Count; i++)
                    //{
                        //info.materialPropertyBlock[i].SetTexture("_BaseMap", newTextur);
                        //info._renderer.SetPropertyBlock(info.materialPropertyBlock[i]);
                    //}
                    break;
                }
            }
        }

        /// <summary>
        /// 初期カラーに戻す（準備しただけ）
        /// </summary>
        /// <param name="renderIndex"></param>
        /// <param name="materialIndex"></param>
        public void SetInitColor(int renderIndex, int materialIndex)
        {
            if (renderInfo.Length < renderIndex) return;
            if (renderInfo[renderIndex].materialPropertyBlock.Count < materialIndex) return;
            RenderInfo target = renderInfo[renderIndex];

            target.materialPropertyBlock[materialIndex].SetColor("_BaseColor", target.initColor[materialIndex]);
            target._renderer.SetPropertyBlock(target.materialPropertyBlock[materialIndex]);
        }

        /// <summary>
        /// 指定カラーに変更（準備しただけ）
        /// </summary>
        /// <param name="renderIndex"></param>
        /// <param name="materialIndex"></param>
        /// <param name="setColor"></param>
        public void SetColor(int renderIndex, int materialIndex, Color setColor)
        {
            if (renderInfo.Length < renderIndex) return;
            if (renderInfo[renderIndex].materialPropertyBlock.Count < materialIndex) return;
            RenderInfo target = renderInfo[renderIndex];

            target.materialPropertyBlock[materialIndex].SetColor("_BaseColor", setColor);
            target._renderer.SetPropertyBlock(target.materialPropertyBlock[materialIndex]);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < renderInfo.Length; i++)
            {
                for (int j = 0; j < renderInfo[i]._renderer.materials.Length; j++)
                {
                    Destroy(renderInfo[i]._renderer.materials[j]);
                }
            }
            renderInfo = null;
        }
    }

    [System.Serializable]
    public class RenderInfo
    {
        public Renderer _renderer;
        public ItemCollisionChecker itemCollisionChecker;//マテリアルオブジェとの接触で色を変える場合は指定する
        public string[] partsName = new string[2] { "partsName", "部位の名前" };//今のとこ未使用
        public List<MaterialPropertyBlock> materialPropertyBlock = new List<MaterialPropertyBlock>();
        public List<Color> initColor = new List<Color>();
    }
}