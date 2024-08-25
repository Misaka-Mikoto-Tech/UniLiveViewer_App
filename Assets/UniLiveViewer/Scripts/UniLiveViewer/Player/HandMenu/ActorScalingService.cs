using MessagePipe;
using System.Linq;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;

namespace UniLiveViewer.Player.HandMenu
{
    /// <summary>
    /// Playerの持っているActor用のリサイズ通知
    /// TODO: OVRGrabberをLS管理にしないと本当のサービスにならない
    /// </summary>
    public class ActorScalingService
    {
        const float ScalingSpeed = 0.01f;

        bool _isUpScaling;
        float _curveTimer;
        InstanceId _targetInstanceId;

        readonly OVRGrabber_UniLiveViewer _hand;
        readonly IPublisher<ActorResizeMessage> _publisher;

        /// <summary>
        /// NonLinearなActor拡縮用
        /// </summary>
        readonly AnimationCurve _animationCurve;
        readonly CompositeDisposable _disposables = new();

        public ActorScalingService(
            OVRGrabber_UniLiveViewer hand,
            IPublisher<ActorResizeMessage> publisher,
            PlayerInputService playerInputService,
            AnimationCurve animationCurve)
        {
            _hand = hand;
            _publisher = publisher;
            _animationCurve = animationCurve;

            hand.HandActionStateAsObservable
                .Where(x => x.Target == HandTargetType.Actor)
                .Subscribe(x =>
                {
                    if (x.Action == HandActionState.Grab)
                    {
                        _targetInstanceId = _hand.GrabbedObj.Value.GetComponent<ActorLifetimeScope>().InstanceId;
                    }
                    else if (x.Action == HandActionState.Release)
                    {
                        _targetInstanceId = null;
                        _curveTimer = 0;
                    }
                }).AddTo(_disposables);

            playerInputService.StickUpAsObservable()
                .Where(x => x == hand.HandType)
                .Subscribe(_ =>
                {
                    if (!_isUpScaling)
                    {
                        _curveTimer = 0;
                        _isUpScaling = true;
                    }
                    OnUpdate(ScalingSpeed);
                }).AddTo(_disposables);
            playerInputService.StickDownAsObservable()
                .Where(x => x == hand.HandType)
                .Subscribe(_ =>
                {
                    if (_isUpScaling)
                    {
                        _curveTimer = 0;
                        _isUpScaling = false;
                    }
                    OnUpdate(-ScalingSpeed);
                }).AddTo(_disposables);
        }

        void OnUpdate(float weight)
        {
            if (_targetInstanceId == null) return;
            _curveTimer += Time.deltaTime;
            var addScale = weight * _animationCurve.Evaluate(_curveTimer);
            _publisher.Publish(new ActorResizeMessage(_targetInstanceId, addScale));
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}