using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRM;
using VRMShaders;

namespace NanaCiel
{
    public static class VRMExpansions
    {
        /// <summary>
        /// サムネイルのみ取得する
        /// TODO: 旧APIだとコレクションエラー出るが解決法あった気がする(NativeArrayManager.cs:64-71)
        /// あとは生パースでもしない限り速度誤差なのでとりまこれで
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<Texture2D> GetThumbnailAsync(string path, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!File.Exists(path)) return null;

            try
            {
                var gltfData = new GlbFileParser(path).Parse();
                var vrmData = new VRMData(gltfData);
                var context = new VRMImporterContext(vrmData);

                var meta = await context.ReadMetaAsync(new RuntimeOnlyAwaitCaller());
                var texture = meta.Thumbnail;
                return texture;
            }
            catch (NotVrm0Exception)
            {

            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("vrmファイルからサムネイル抽出中に中断");
                throw;
            }
            return null;
        }
    }
}
