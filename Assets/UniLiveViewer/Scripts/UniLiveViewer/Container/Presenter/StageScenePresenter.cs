using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    /// <summary>
    /// シーン遷移・ファイル準備のみ
    /// </summary>
    public class StageScenePresenter : IAsyncStartable
    {
        readonly SceneChangeService _sceneChangeService;
        readonly FileAccessManager _fileAccessManager;
        readonly OVRScreenFade _ovrScreenFade;
        readonly AnimationAssetManager _animationAssetManager;
        readonly TextureAssetManager _textureAssetManager;

        [Inject]
        public StageScenePresenter(
            FileAccessManager fileAccessManager,
            AnimationAssetManager animationAssetManager,
            TextureAssetManager textureAssetManager,
            SceneChangeService sceneChangeService)
        {
            _sceneChangeService = sceneChangeService;
            _fileAccessManager = fileAccessManager;
            _animationAssetManager = animationAssetManager;
            _textureAssetManager = textureAssetManager;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            UnityEngine.Debug.Log("Trace: StageScenePresenter.StartAsync");


            _sceneChangeService.SetupFirstScene();


            await _fileAccessManager.PreparationStart(cancellation).OnError(OnFolderError);
            _animationAssetManager.Setup();
            await _textureAssetManager.CacheThumbnails(cancellation).OnError(OnThumbnailsError);
            _fileAccessManager.PreparationEnd();

            //await _fileAccessManager.OnStartAsync(_animationAssetManager, _textureAssetManager, cancellation);

            UnityEngine.Debug.Log("Trace: StageScenePresenter.StartAsync");
        }

        void OnFolderError(Exception e)
        {
            Debug.Log($"フォルダ準備エラー:{e}");
        }

        void OnThumbnailsError(Exception e)
        {
            Debug.Log($"サムネイルチェックエラー:{e}");
        }
    }
}
