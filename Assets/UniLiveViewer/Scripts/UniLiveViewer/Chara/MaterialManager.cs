using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class MaterialManager : MonoBehaviour
    {
        //マテリアル名とskinmeshの中央座標(localoffset)を管理
        public Dictionary<string, Vector3> matLocation = new Dictionary<string, Vector3>();
        public List<MaterialInfo> info = new List<MaterialInfo>();

        public class MaterialInfo
        {
            public string name;
            public SkinnedMeshRenderer skinMesh;
            public int index;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        /// <summary>
        /// マテリアル情報を抽出
        /// </summary>
        /// <param name="_skinMesh"></param>
        public async UniTask ExtractMaterials(CharaController charaCon,CancellationToken token)
        {
            foreach (var e in charaCon.GetSkinnedMeshRenderers)
            {
                AddMaterialInfo(e);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private void AddMaterialInfo(SkinnedMeshRenderer _skinMesh)
        {
            MaterialInfo _info;
            string matName = "";
            for (int i = 0; i < _skinMesh.materials.Length; i++)
            {
                matName = _skinMesh.materials[i].name;

                //重複排除で登録
                if (!matLocation.ContainsKey(matName))
                {
                    //Debug.Log("----辞書に新規追加:" + matName + "--------");
                    matLocation.Add(matName, _skinMesh.bounds.center - transform.position);
                }

                _info = new MaterialInfo();
                _info.name = matName;
                _info.skinMesh = _skinMesh;
                _info.index = i;
                info.Add(_info);

                //Debug.Log($"リスト追加->{_skinMesh}/{i}番目/{matName}");
            }
        }

        public void SetSurface(string name, SurfaceType type)
        {
            int _index;
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].name == name)
                {
                    _index = info[i].index;
                    info[i].skinMesh.materials[_index].SetFloat("_Surface", (float)type);
                }
            }
        }

        public void SetRenderFace(string name, RenderFace render)
        {
            int _index;
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].name == name)
                {
                    _index = info[i].index;
                    info[i].skinMesh.materials[_index].SetFloat("_Cull", (float)render);
                }
            }
        }

        public void SetCutoff(string name, int val)
        {
            int _index;
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].name == name)
                {
                    _index = info[i].index;
                    info[i].skinMesh.materials[_index].SetFloat("_AlphaClip", val);
                }
            }
        }

        public void SetColor_Transparent(string name, float alpha)
        {
            int _index;
            Color col;
            for (int i = 0;i< info.Count;i++)
            {
                if (info[i].name == name)
                {
                    _index = info[i].index;
                    col = info[i].skinMesh.materials[_index].GetColor("_Color");
                    col.a = alpha;
                    info[i].skinMesh.materials[_index].SetColor("_Color", col);

                    col = info[i].skinMesh.materials[_index].GetColor("_ShadeColor");
                    col.a = alpha;
                    info[i].skinMesh.materials[_index].SetColor("_ShadeColor", col);
                }
            }
        }

        public void SetCutoffVal(string name, float val)
        {
            int _index;
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].name == name)
                {
                    _index = info[i].index;
                    info[i].skinMesh.materials[_index].SetFloat("_Cutoff", val);
                }
            }
        }
    }

}