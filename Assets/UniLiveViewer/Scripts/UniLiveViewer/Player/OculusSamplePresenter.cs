using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    public class OculusSamplePresenter : IStartable, IDisposable
    {
        readonly IPublisher<AttachPointMessage> _publisher;
        readonly FileAccessManager _fileAccessManager;
        readonly PlayerStateManager _playerStateManager;
        readonly List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public OculusSamplePresenter(
            IPublisher<AttachPointMessage> publisher,
            FileAccessManager fileAccessManager,
            PlayerStateManager playerStateManager,
            List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _publisher = publisher;
            _fileAccessManager = fileAccessManager;
            _playerStateManager = playerStateManager;
            
            _ovrGrabbers = ovrGrabbers;
        }

        void IStartable.Start()
        {
            foreach (var ovrGrabber in _ovrGrabbers)
            {
                ovrGrabber.HandActionStateAsObservable
                    .Where(x => x.Target == HandTargetType.Item)
                    .Subscribe(x =>
                    {
                        if (x.Action == HandActionState.Grab)
                        {
                            var data = new AttachPointMessage(true);
                            _publisher.Publish(data);
                        }
                        // MEMO: Statemanagerで両手checkしてるのでこれ多分不要
                        //else if (x.Action == HandActionState.Release)
                        //{
                        //    var data = new AttachPointMessage(false);
                        //    _publisher.Publish(data);
                        //}
                    }).AddTo(_disposables);
            }
            // 離す時は両手確認
            _playerStateManager.CompletelyReleasedItemAsObservable
                .Select(x => new AttachPointMessage(false))
                .Subscribe(_publisher.Publish).AddTo(_disposables);

            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _playerStateManager.enabled = true)
                .AddTo(_disposables);

            _playerStateManager.enabled = false;

            
            _playerStateManager.OnStart();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
            _playerStateManager.Dispose();
        }
    }
}