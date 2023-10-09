using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    public class OculusSamplePresenter : IStartable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;

        readonly PlayerStateManager _playerStateManager;
        readonly LocomotionRestrictionService _movementRestrictionService;
        readonly PassthroughService _passthroughService;
        readonly HandUIController _handUIController;

        readonly CompositeDisposable _disposables;

        [Inject]
        public OculusSamplePresenter(
            FileAccessManager fileAccessManager,
            PlayerStateManager playerStateManager,
            LocomotionRestrictionService movementRestrictionService,
            PassthroughService passthroughService,
            HandUIController handUIController)
        {
            _fileAccessManager = fileAccessManager;
            _playerStateManager = playerStateManager;
            _movementRestrictionService = movementRestrictionService;
            _passthroughService = passthroughService;
            _handUIController = handUIController;

            _disposables = new CompositeDisposable();
        }

        void IStartable.Start()
        {
            UnityEngine.Debug.Log("Trace: OculusSamplePresenter.Start");

            _playerStateManager.enabled = false;

            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _playerStateManager.enabled = true)
                .AddTo(_disposables);

            _playerStateManager.PlayerInputAsObservable
                .Subscribe(_ => _movementRestrictionService.MovementRestrictions())
                .AddTo(_disposables);

            _passthroughService.OnStart();
            _playerStateManager.OnStart();
            _handUIController.OnStart();

            UnityEngine.Debug.Log("Trace: OculusSamplePresenter.Start");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

}