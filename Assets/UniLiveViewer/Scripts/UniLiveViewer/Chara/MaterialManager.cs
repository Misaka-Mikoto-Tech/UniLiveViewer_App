using Cysharp.Threading.Tasks;
using System.Collections;
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
        public async UniTask ExtractMaterials(SkinnedMeshRenderer[] _skinMeshs,CancellationToken token)
        {
            foreach (var e in _skinMeshs)
            {
                AddMaterialInfo(e);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            foreach (var e in info)
            {
                Debug.Log($"リスト追加->{e.skinMesh}/{e.index}番目/{e.name}");
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
    }

}