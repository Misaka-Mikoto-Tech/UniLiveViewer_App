using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class ThumbnailPresenter : IAsyncStartable, IDisposable
    {
        readonly ThumbnailController _thumbnailController;
        readonly VRMSwitchController _switchController;
        readonly TextureAssetManager _textureAssetManager;
        readonly Transform _thumbnailRoot;

        readonly CompositeDisposable _disposables;

        [Inject]
        public ThumbnailPresenter(
            ThumbnailController thumbnailController,
            VRMSwitchController switchController,
            TextureAssetManager textureAssetManager,
            Transform thumbnailRoot)
        {
            _thumbnailController = thumbnailController;
            _switchController = switchController;
            _textureAssetManager = textureAssetManager;
            _thumbnailRoot = thumbnailRoot;

            _disposables = new CompositeDisposable();
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Trace: ThumbnailPresenter.Start");

            _thumbnailController.OnGeneratedAsObservable
                .Subscribe(_ => _switchController.OnGeneratedThumbnail(cancellation))
                .AddTo(_disposables);
            _thumbnailController.OnClickAsObservable
                .Select(x => x.name)
                .Subscribe(x => {
                    //重複クリックできないようにボタンを無効化
                    _thumbnailRoot.gameObject.SetActive(false);

                    _switchController.OnClickThumbnail(x, cancellation);
                })
                .AddTo(_disposables);

            _thumbnailController.OnStart(_textureAssetManager, _thumbnailRoot, cancellation);

            Debug.Log("Trace: ThumbnailPresenter.Start");

            await UniTask.CompletedTask;
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
