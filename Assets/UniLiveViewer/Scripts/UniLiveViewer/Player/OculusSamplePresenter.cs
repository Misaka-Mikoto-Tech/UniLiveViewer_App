using MessagePipe;
using System;
using System.Collections.Generic;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniRx;
using VContainer;
using VContainer.Unity;
using System.Linq;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// TODO: もう少し役割分散
    /// </summary>
    public class OculusSamplePresenter : IStartable, IDisposable
    {
        readonly IPublisher<AttachPointMessage> _publisher;
        readonly FileAccessManager _fileAccessManager;
        readonly PlayerStateManager _playerStateManager;
        readonly LocomotionRestrictionService _movementRestrictionService;
        readonly PassthroughService _passthroughService;
        readonly HandUIController _handUIController;
        readonly List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        readonly CompositeDisposable _disposables = new();
        /// <summary>
        /// 常に1つだけ購読用
        /// </summary>
        readonly SerialDisposable _serialDisposable = new();

        [Inject]
        public OculusSamplePresenter(
            IPublisher<AttachPointMessage> publisher,
            FileAccessManager fileAccessManager,
            PlayerStateManager playerStateManager,
            LocomotionRestrictionService movementRestrictionService,
            PassthroughService passthroughService,
            HandUIController handUIController,
            List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _publisher = publisher;
            _fileAccessManager = fileAccessManager;
            _playerStateManager = playerStateManager;
            _movementRestrictionService = movementRestrictionService;
            _passthroughService = passthroughService;
            _handUIController = handUIController;
            _ovrGrabbers = ovrGrabbers;
        }

        void IStartable.Start()
        {
            _playerStateManager.GrabbedItemAsObservable
                .Select(x => new AttachPointMessage(x))
                .Subscribe(_publisher.Publish).AddTo(_disposables);

            foreach (var ovrGrabber in _ovrGrabbers)
            {
                ovrGrabber.GrabbedItemAsObservable
                    .Select(x => new AttachPointMessage(x))
                    .Subscribe(_publisher.Publish).AddTo(_disposables);

                ovrGrabber.GrabbedObj
                    .Subscribe(OnChangeGrabbedObj).AddTo(_disposables);
            }

            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _playerStateManager.enabled = true)
                .AddTo(_disposables);

            _playerStateManager.PlayerInputAsObservable
                .Subscribe(_ => _movementRestrictionService.MovementRestrictions())
                .AddTo(_disposables);

            _playerStateManager.enabled = false;

            _passthroughService.OnStart();
            _playerStateManager.OnStart();
            _handUIController.OnStart();
        }

        /// <summary>
        /// 握っている間のみ購読
        /// </summary>
        /// <param name="ovrGrabbableCustom"></param>
        void OnChangeGrabbedObj(OVRGrabbable_Custom ovrGrabbableCustom)
        {
            if(ovrGrabbableCustom == null)
            {
                _serialDisposable.Disposable = null;
            }
            else if(ovrGrabbableCustom.TryGetComponent<Actor.ActorLifetimeScope>(out var actor))
            {
                var actorService = actor.Container.Resolve<Actor.IActorService>();

                _serialDisposable.Disposable = actorService.RootScalar()
                .Subscribe(x =>
                {
                    _handUIController.OnChangeActorSize(x);
                });
            }
            else
            {
                _serialDisposable.Disposable = null;
            }
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }

}