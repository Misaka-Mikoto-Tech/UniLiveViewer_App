using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.General
{
    /// <summary>
    /// MEMO: MenuLifetimeScopeを直接GripするとSpriteが崩れる
    /// </summary>
    public class PositionHandlerLifetimeScope : LifetimeScope
    {
        [SerializeField] Renderer _renderer;
        [SerializeField] PositionHandlerSrcAnchor _srcAnchor;
        [SerializeField] PositionHandlerDestAnchor _destAnchor;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_renderer);
            builder.RegisterComponent(_srcAnchor);
            builder.RegisterComponent(_destAnchor);
            builder.Register<PositionHandlerService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PositionHandlerPresenter>();
        }

        void OnEnable()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            _srcAnchor.transform.localPosition = Vector3.zero;
            _srcAnchor.transform.localRotation = Quaternion.identity;
        }
    }
}
