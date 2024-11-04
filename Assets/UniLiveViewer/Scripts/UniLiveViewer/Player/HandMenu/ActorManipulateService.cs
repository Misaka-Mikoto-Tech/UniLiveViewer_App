using UnityEngine;
using VContainer;

namespace UniLiveViewer.Player.HandMenu
{
    public class ActorManipulateService
    {
        UniLiveViewer.HandMenu[] _handMenu = new UniLiveViewer.HandMenu[2];

        bool _isSetupComplete;

        readonly PlayerHandMenuAnchorL _playerHandMenuAnchorL;
        readonly PlayerHandMenuAnchorR _playerHandMenuAnchorR;
        readonly PlayerHandMenuSettings _playerHandMenuSettings;
        readonly Transform _lookTarget;

        [Inject]
        public ActorManipulateService(
            PlayerHandMenuAnchorL playerHandMenuAnchorL,
            PlayerHandMenuAnchorR playerHandMenuAnchorR,
            PlayerHandMenuSettings playerHandMenuSettings,
            Camera camera)
        {
            _playerHandMenuAnchorL = playerHandMenuAnchorL;
            _playerHandMenuAnchorR = playerHandMenuAnchorR;
            _playerHandMenuSettings = playerHandMenuSettings;
            _lookTarget = camera.transform;
        }

        public void Setup()
        {
            _handMenu[0] = new UniLiveViewer.HandMenu(
                GameObject.Instantiate(_playerHandMenuSettings.ActorManipulate),
                _playerHandMenuAnchorL.transform);
            _handMenu[1] = new UniLiveViewer.HandMenu(
                GameObject.Instantiate(_playerHandMenuSettings.ActorManipulate),
                _playerHandMenuAnchorR.transform);

            var text = $"{FileReadAndWriteUtility.UserProfile.InitCharaSize}0.00";
            foreach (var handMenu in _handMenu)
            {
                handMenu.SetText(text);
                handMenu.SetShow(false);
            }

            _isSetupComplete = true;
        }

        public void ChangeShow(PlayerHandType handType, bool isShow)
        {
            var index = handType == PlayerHandType.LHand ? 0 : 1;
            if (_handMenu.Length <= index) return;
            _handMenu[index].SetShow(isShow);
        }

        public void OnChangeActorSize(float size)
        {
            var text = $"{size:0.00}";
            foreach (var handMenu in _handMenu)
            {
                handMenu.SetText(text);
            }
        }

        public void OnLateTick()
        {
            if (!_isSetupComplete) return;

            foreach (var handMenu in _handMenu)
            {
                handMenu.UpdateLookat(_lookTarget);
            }
        }

        public bool IsShowAny()
        {
            foreach (var handMenu in _handMenu)
            {
                if (handMenu.IsShow) return true;
            }
            return false;
        }
    }
}