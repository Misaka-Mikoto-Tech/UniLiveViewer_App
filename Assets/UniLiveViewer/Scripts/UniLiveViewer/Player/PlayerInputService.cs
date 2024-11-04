using System;
using UniRx;
using UnityEngine;
using VContainer;
using static UniLiveViewer.PlayerConfigData;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// NOTE: OVRInputだとeventがない...？
    /// </summary>
    public class PlayerInputService
    {
        /// <summary>
        /// 左：サブメニュー / 右：メインメニュー
        /// </summary>
        /// <returns></returns>
        public IObservable<PlayerHandType> ClickMenuAsObservable() => _clickMenuStream;
        readonly Subject<PlayerHandType> _clickMenuStream = new();
        public IObservable<PlayerHandType> ClickActionAsObservable() => _clickActionStream;
        readonly Subject<PlayerHandType> _clickActionStream = new();
        public IObservable<PlayerHandType> ClickTriggerAsObservable() => _clickTriggerStream;
        readonly Subject<PlayerHandType> _clickTriggerStream = new();

        public IObservable<PlayerHandType> ClickStickLeftAsObservable() => _clickStickLeftStream;
        readonly Subject<PlayerHandType> _clickStickLeftStream = new();
        public IObservable<PlayerHandType> ClickStickRightAsObservable() => _clickStickRightStream;
        readonly Subject<PlayerHandType> _clickStickRightStream = new();
        public IObservable<PlayerHandType> ClickStickUpAsObservable() => _clickStickUpStream;
        readonly Subject<PlayerHandType> _clickStickUpStream = new();
        public IObservable<PlayerHandType> ClickStickDownAsObservable() => _clickStickDownStream;
        readonly Subject<PlayerHandType> _clickStickDownStream = new();

        public IObservable<PlayerHandType> StickUpAsObservable() => _stickUpStream;
        readonly Subject<PlayerHandType> _stickUpStream = new();
        public IObservable<PlayerHandType> StickDownAsObservable() => _stickDownStream;
        readonly Subject<PlayerHandType> _stickDownStream = new();

        public IReadOnlyReactiveProperty<Vector2> LeftStickInput() => _leftStickInput;
        readonly ReactiveProperty<Vector2> _leftStickInput = new();
        public IReadOnlyReactiveProperty<Vector2> RightStickInput() => _rightStickInput;
        readonly ReactiveProperty<Vector2> _rightStickInput = new();

        readonly KeyConfig _leftKeyConfig;
        readonly KeyConfig _rightKeyConfig;
        readonly DebugKeyConfig _debugKey;

        [Inject]
        public PlayerInputService(
            PlayerConfigData playerConfigData)
        {
            _leftKeyConfig = playerConfigData.LeftKeyConfig;
            _rightKeyConfig = playerConfigData.RightKeyConfig;
            _debugKey = playerConfigData.DebugKey;
        }

        public void OnTick()
        {
            if (OVRInput.GetDown(_leftKeyConfig.menuUI))
            {
                _clickMenuStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.menuUI))
            {
                _clickMenuStream.OnNext(PlayerHandType.RHand);
            }

            if (OVRInput.GetDown(_leftKeyConfig.action))
            {
                _clickActionStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.action))
            {
                _clickActionStream.OnNext(PlayerHandType.RHand);
            }

            if (OVRInput.GetDown(_leftKeyConfig.trigger))
            {
                _clickTriggerStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.trigger))
            {
                _clickTriggerStream.OnNext(PlayerHandType.RHand);
            }

            if (OVRInput.GetDown(_leftKeyConfig.rotate_L))
            {
                _clickStickLeftStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.rotate_L))
            {
                _clickStickLeftStream.OnNext(PlayerHandType.RHand);
            }

            if (OVRInput.GetDown(_leftKeyConfig.rotate_R))
            {
                _clickStickRightStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.rotate_R))
            {
                _clickStickRightStream.OnNext(PlayerHandType.RHand);
            }

            if (OVRInput.GetDown(_leftKeyConfig.resize_U))
            {
                _clickStickUpStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.resize_U))
            {
                _clickStickUpStream.OnNext(PlayerHandType.RHand);
            }
            if (OVRInput.GetDown(_leftKeyConfig.resize_D))
            {
                _clickStickDownStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.GetDown(_rightKeyConfig.resize_D))
            {
                _clickStickDownStream.OnNext(PlayerHandType.RHand);
            }

            if (OVRInput.Get(_leftKeyConfig.resize_U))
            {
                _stickUpStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.Get(_rightKeyConfig.resize_U))
            {
                _stickUpStream.OnNext(PlayerHandType.RHand);
            }
            if (OVRInput.Get(_leftKeyConfig.resize_D))
            {
                _stickDownStream.OnNext(PlayerHandType.LHand);
            }
            if (OVRInput.Get(_rightKeyConfig.resize_D))
            {
                _stickDownStream.OnNext(PlayerHandType.RHand);
            }

            _leftStickInput.Value = OVRInput.Get(_leftKeyConfig.thumbstick);
            _rightStickInput.Value = OVRInput.Get(_rightKeyConfig.thumbstick);

            DebugInput();
        }

        void DebugInput()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(_debugKey.MainMenu))
            {
                _clickMenuStream.OnNext(PlayerHandType.RHand);
            }
#endif
        }
    }
}