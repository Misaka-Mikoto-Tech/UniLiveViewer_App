using System;
using UniLiveViewer;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class OculusSamplePresenter : IStartable, IDisposable
{
    readonly Camera _camera;
    readonly OVRManager _ovrManager;
    readonly PlayerStateManager _playerStateManager;
    readonly MovementRestrictionService _movementRestrictionService;
    readonly PassthroughService _passthroughService;
    readonly HandUIController _handUIController;
    readonly CharacterCameraConstraint_Custom _characterCameraConstraintCustom;

    readonly CompositeDisposable _disposables;

    [Inject]
    public OculusSamplePresenter(Camera camera,
        OVRManager ovrManager,
        PlayerStateManager playerStateManager,
        MovementRestrictionService movementRestrictionService,
        PassthroughService passthroughService,
        HandUIController handUIController,
        CharacterCameraConstraint_Custom characterCameraConstraintCustom)
    {
        _camera = camera;
        _ovrManager = ovrManager;
        _playerStateManager = playerStateManager;
        _movementRestrictionService = movementRestrictionService;
        _passthroughService = passthroughService;
        _handUIController = handUIController;
        _characterCameraConstraintCustom = characterCameraConstraintCustom;

        _disposables = new CompositeDisposable();
    }

    void IStartable.Start()
    {
        UnityEngine.Debug.Log("Trace: OculusSamplePresenter.Start");

        _playerStateManager.PlayerInputAsObservable
            .Subscribe(_ => _movementRestrictionService.MovementRestrictions())
            .AddTo(_disposables);

        _passthroughService.Initialize(_ovrManager, _camera);
        _playerStateManager.Initialize(_handUIController);
        _handUIController.Initialize(_characterCameraConstraintCustom, _camera);

        UnityEngine.Debug.Log("Trace: OculusSamplePresenter.Start");
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
