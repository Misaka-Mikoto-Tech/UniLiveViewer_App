﻿using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace UniLiveViewer.SceneLoader
{
    public class KaguraLiveScene : IScene
    {
        const int BufferTime = 5000;
        const string SceneName = "KAGURAScene";
        public KaguraLiveScene()
        {
        }
        
        async UniTask IScene.BeginAsync(CancellationToken token)
        {
            //完全非同期は無理
            var async = SceneManager.LoadSceneAsync(SceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(BufferTime, cancellationToken: token);
            async.allowSceneActivation = true;
        }

        string IScene.GetVisualName() => "★KAGURA Live★";
    }
}
