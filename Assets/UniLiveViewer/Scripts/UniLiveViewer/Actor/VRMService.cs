using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UniGLTF;
using UnityEngine;
using UniVRM10;
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
        /// 簡易版APIでロード
        /// VRM1.0だとさらに完結に
        /// </summary>
        /// <param name="path"></param>
        public async UniTask<RuntimeGltfInstance> LoadAsync(string path, CancellationToken cancellation)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return null;
                if (!File.Exists(path))
                {
                    Debug.LogWarning("Specified file does not exist");
                    return null;
                }
                var bytes = File.ReadAllBytes(path);
                var size = bytes != null ? bytes.Length : 0;
                var instance = await VrmUtility.LoadBytesAsync(path, bytes, GetIAwaitCaller(true));
                await UniTask.Yield(cancellation);// 負荷分散
                return instance;
            }
            catch (NotVrm0Exception)
            {
                Debug.LogWarning("1.0無理だよぉ...");
                throw;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("VRM load canceled");
                throw;
            }
            catch
            {
                Debug.LogWarning("vrm some kind of error");
                throw;
            }
        }

        public async UniTask<Vrm10Instance> Load10Async(string path, CancellationToken cancellation)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return null;
                if (!File.Exists(path))
                {
                    Debug.LogWarning("Specified file does not exist");
                    return null;
                }
                var vrm10Instance = await Vrm10.LoadPathAsync(path,
                    canLoadVrm0X: true,
                    showMeshes: false,
                    awaitCaller: GetIAwaitCaller(true),
                    materialGenerator: GetVrmMaterialDescriptorGenerator(true),//VRM10は強制URPにする
                    vrmMetaInformationCallback: null,
                    ct: cancellation);

                await UniTask.Yield(cancellation);// 負荷分散
                return vrm10Instance;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("VRM load canceled");
                throw;
            }
            catch
            {
                Debug.LogWarning("vrm some kind of error");
                throw;
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

        static IMaterialDescriptorGenerator GetVrmMaterialDescriptorGenerator(bool useUrp)
        {
            if (useUrp)
            {
                return new UrpVrm10MaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInVrm10MaterialDescriptorGenerator();
            }
        }
    }
}
