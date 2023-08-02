using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using VRM;
using VRMShaders;


namespace NanaCiel
{
    /// <summary>
    /// 旧公式サンプルをURP用に改造したもの、UniVRM更新したら多分もういらない
    /// </summary>
    public static class VRMExpansions
    {
        /// <summary>
        /// サムネイルのみ取得する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<Texture2D> GetThumbnail(string path, CancellationToken cancellation)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!File.Exists(path)) return null;

            // GLB形式でJSONを取得しParseします
            var data = new GlbFileParser(path).Parse();

            try
            {
                // VRM extension を parse します
                var vrm = new VRMData(data);

                using (var loader = new VRMImporterContext(vrm))
                {
                    //サムネイルだけ取得する
                    return await loader.ReadMetaAsync_Thumbnail(new ImmediateCaller(), true);
                    //VRMMetaObject vrmMetaObject = await loader.ReadMetaAsync(new ImmediateCaller(), true);
                    //result = vrmMetaObject.Thumbnail;
                }
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

        //public async Task<Texture2D> GetThumbnail(string path)
        //{
        //    Texture2D result = null;

        //    if (string.IsNullOrEmpty(path)) return result;
        //    if (!File.Exists(path)) return result;

        //    // GLB形式でJSONを取得しParseします
        //    var data = new GlbFileParser(path).Parse();

        //    try
        //    {
        //        // VRM extension を parse します
        //        var vrm = new VRMData(data);

        //        using (var loader = new VRMImporterContext(vrm))
        //        {
        //            //サムネイルだけ取得する
        //            result = await loader.ReadMetaAsync_Thumbnail(new ImmediateCaller(), true);
        //            //VRMMetaObject vrmMetaObject = await loader.ReadMetaAsync(new ImmediateCaller(), true);
        //            //result = vrmMetaObject.Thumbnail;
        //        }
        //    }
        //    catch (NotVrm0Exception)
        //    {

        //    }
        //    return result;
        //}
    }
}
