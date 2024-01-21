using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniGLTF;
using UnityEngine;
using VContainer;
using VRM;
using VRMShaders;

namespace UniLiveViewer.Actor
{
    public class VRMService
    {
        public VRMService()
        {
        }

        /// <summary>
        /// URPモードでVRMのみを読み込む
        /// </summary>
        /// <param name="path"></param>
        public async UniTask<RuntimeGltfInstance_Custom> LoadAsync(string path, CancellationToken cancellation)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }
                var data = new GlbFileParser(path).Parse();
                var vrm = new VRMData(data);
                var context = new VRMImporterContext_Custom(vrm, materialGenerator: GetVrmMaterialGenerator(false, vrm.VrmExtension));

                var loaded = default(RuntimeGltfInstance_Custom);
                loaded = await context.LoadAsync(GetIAwaitCaller(true));
                await UniTask.Yield(cancellation);// 負荷分散
                return loaded;
            }
            catch (OperationCanceledException)
            {
                Debug.Log("キャンセルされた");
                throw;
            }
            catch
            {
                Debug.Log("VRM Loadエラー");
                throw;
            }
        }

        /// <summary>
        /// URP読み込みに必要
        /// </summary>
        /// <param name="useUrp"></param>
        /// <param name="vrm"></param>
        /// <returns></returns>
        static IMaterialDescriptorGenerator GetVrmMaterialGenerator(bool useUrp, glTF_VRM_extensions vrm)
        {
            if (useUrp)
            {
                return new VRMUrpMaterialDescriptorGenerator(vrm);
            }
            else
            {
                return new VRMMaterialDescriptorGenerator(vrm);
            }
        }

        /// <summary>
        /// 非同期読み込みに必要
        /// </summary>
        /// <param name="useAsync"></param>
        /// <returns></returns>
        static IAwaitCaller GetIAwaitCaller(bool useAsync)
        {
            if (useAsync)
            {
                return new RuntimeOnlyAwaitCaller();
            }
            else
            {
                return new ImmediateCaller();
            }
        }
    }
}
