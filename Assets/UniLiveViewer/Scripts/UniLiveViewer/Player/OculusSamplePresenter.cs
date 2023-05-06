using System;
using UniLiveViewer;
using UniRx;
using VContainer;
using VContainer.Unity;

public class OculusSamplePresenter : IStartable, IDisposable
{
    readonly PlayerStateManager _playerStateManager;
    readonly MovementRestrictionService _movementRestrictionService;

    readonly CompositeDisposable _disposables;

    [Inject]
    public OculusSamplePresenter(
        PlayerStateManager playerStateManager,
        MovementRestrictionService movementRestrictionService)
    {
        _playerStateManager = playerStateManager;
        _movementRestrictionService = movementRestrictionService;

        _disposables = new CompositeDisposable();
    }

    void IStartable.Start()
    {
        _playerStateManager.PlayerInputAsObservable
            .Subscribe(_ => _movementRestrictionService.MovementRestrictions())
            .AddTo(_disposables);
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
