using VContainer;
using VContainer.Unity;
using UniLiveViewer;
using UnityEngine;

public class PlayerLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        builder.RegisterComponent<Camera>(camera);

        builder.Register<MovementRestrictionService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<PassthroughService>();

        builder.RegisterComponentInHierarchy<PlayerStateManager>();
        builder.RegisterComponentInHierarchy<SimpleCapsuleWithStickMovement>();
        builder.RegisterComponentInHierarchy<HandUIController>();

        builder.RegisterEntryPoint<OculusSamplePresenter>();
    }
}
