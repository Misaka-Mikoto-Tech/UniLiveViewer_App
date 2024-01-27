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
        /// あとは直パースでもしない限り速度誤差なのでとりまこれで
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<Texture2D> GetThumbnailAsync(string path, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!File.Exists(path)) return null;

            try
            {
                //https://indie-du.com/entry/2020/11/10/094145
                using (var gltfData = new GlbFileParser(path).Parse())
                {
                    var context = new VRMImporterContext(new VRMData(gltfData));
                    var meta = await context.ReadMetaAsync(new RuntimeOnlyAwaitCaller());
                    var texture = meta.Thumbnail;
                    return texture;
                }   
            }
            catch (NotVrm0Exception)
            {
                Debug.LogWarning("1.0無理だよぉ...");
            }
            catch (System.OperationCanceledException)
            {
                Debug.LogWarning("Thumbnail extraction canceled");
            }
            catch
            {
                Debug.LogWarning("vrm some kind of error");
            }
            return null;
        }
    }
}
