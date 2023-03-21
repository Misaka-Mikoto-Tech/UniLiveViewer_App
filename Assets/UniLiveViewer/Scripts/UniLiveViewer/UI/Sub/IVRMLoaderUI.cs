
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// サンプルアクセス用
    /// </summary>
    public interface IVRMLoaderUI
    {
        void SetUIActive(bool b);
        /// <summary>
        /// TODO: 直バイナリパース実装してUIと関係を断ちたい
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellation_token"></param>
        /// <returns></returns>
        Task<Texture2D> GetThumbnailAsync(string path, CancellationToken cancellation_token);
        Task<RuntimeGltfInstance_Custom> GetURPVRMAsync(string path, CancellationToken cancellation_token);
        /// <summary>
        /// VRMをプレハブ化する
        /// </summary>
        /// <param name="instance"></param>
        void SetVRMToPrefab(CharaController vrm);
        /// <summary>
        /// プレハブ化しているVRMを削除する
        /// </summary>
        /// <param name="id"></param>
        void DeleteVRMPrefab(int id);
    }
}