using System.Collections.Generic;
using UniLiveViewer.Player.State;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.HandMenu
{

    public class PlayerHandMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] AudioSourceService _audioSourceService;
        [SerializeField] PlayerHandMenuSettings _playerHandMenuSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_audioSourceService);
            builder.RegisterComponent(_playerHandMenuSettings);
            
            builder.Register<CameraHeightService>(Lifetime.Singleton);
            builder.Register<ActorManipulateService>(Lifetime.Singleton);
            builder.Register<ItemMaterialSelectionService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerHandMenuPresenter>();

            builder.Register<MovableState>(Lifetime.Singleton);
            builder.Register<NonMovableState>(Lifetime.Singleton);
            builder.Register<PlayerStateMachineService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlayerStatePresenter>();
        }
    }
}