using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UniGLTF;
using UnityEngine;
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
        /// VRM1.0だとさらに完結に、旧APIはエラー出るので避ける
        /// </summary>
        /// <param name="path"></param>
        public async UniTask<RuntimeGltfInstance> LoadAsync(string path, CancellationToken cancellation)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return null;
                if (!File.Exists(path))
                {
                    Debug.LogWarning("指定ファイルが存在しない");
                    return null;
                }
                var bytes = File.ReadAllBytes(path);
                var size = bytes != null ? bytes.Length : 0;
                var instance = await VrmUtility.LoadBytesAsync(path, bytes, GetIAwaitCaller(true));
                await UniTask.Yield(cancellation);// 負荷分散
                return instance;
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
