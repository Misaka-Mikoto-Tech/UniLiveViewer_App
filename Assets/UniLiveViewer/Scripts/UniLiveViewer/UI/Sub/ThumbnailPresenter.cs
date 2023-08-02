using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class ThumbnailPresenter : IStartable
    {
        readonly ThumbnailController _thumbnailController;
        readonly TextureAssetManager _textureAssetManager;

        [Inject]
        public ThumbnailPresenter(
            ThumbnailController thumbnailController,
            TextureAssetManager textureAssetManager)
        {
            _thumbnailController = thumbnailController;
            _textureAssetManager = textureAssetManager;
        }

        void IStartable.Start()
        {
            UnityEngine.Debug.Log("Trace: ThumbnailPresenter.Start");

            _thumbnailController.Initialize(_textureAssetManager);

            UnityEngine.Debug.Log("Trace: ThumbnailPresenter.Start");
        }
    }
}
