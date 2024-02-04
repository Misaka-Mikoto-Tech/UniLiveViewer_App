using UniLiveViewer.OVRCustom;
using UnityEngine;
using VContainer;
using static UniLiveViewer.PlayerConfigData;

namespace UniLiveViewer.Player.HandMenu
{
    public class CameraHeightService
    {
        UniLiveViewer.HandMenu _handMenu;

        float _height;
        bool _isSetupComplete;

        readonly CharacterCameraConstraint_Custom _characterCameraConstraintCustom;
        readonly KeyConfig _keyConfig;
        readonly PlayerHandMenuAnchorL _playerHandMenuAnchorL;
        readonly PlayerHandMenuSettings _playerHandMenuSettings;
        readonly Transform _lookTarget;

        [Inject]
        public CameraHeightService(
            CharacterCameraConstraint_Custom characterCameraConstraintCustom,
            PlayerConfigData playerConfigData,
            PlayerHandMenuAnchorL playerHandMenuAnchorL,
            PlayerHandMenuSettings playerHandMenuSettings,
            Camera camera)
        {
            _characterCameraConstraintCustom = characterCameraConstraintCustom;
            _keyConfig = playerConfigData.LeftKeyConfig;
            _playerHandMenuAnchorL = playerHandMenuAnchorL;
            _playerHandMenuSettings = playerHandMenuSettings;
            _lookTarget = camera.transform;
        }

        public void Setup()
        {
            _handMenu = new UniLiveViewer.HandMenu(
                GameObject.Instantiate(_playerHandMenuSettings.CameraHeighte),
                _playerHandMenuAnchorL.transform);
            SetCameraHeight(_characterCameraConstraintCustom.HeightOffset);
            _handMenu.SetShow(false);

            _isSetupComplete = true;
        }

        void SetCameraHeight(float height)
        {
            _height = Mathf.Clamp(height, 0, 2.0f);
            _characterCameraConstraintCustom.HeightOffset = _height;
            var text = $"{_height:0.00}";
            _handMenu.SetText(text);
        }

        public void ChangeShow()
        {
            _handMenu.SetShow(!_handMenu.IsShow);
        }

        public void OnLateTick()
        {
            if (!_isSetupComplete) return;
            if (!_handMenu.IsShow) return;

            //Playerカメラの高さ調整
            if (OVRInput.GetDown(_keyConfig.resize_U))
            {
                _height += 0.05f;
                SetCameraHeight(_height);
            }
            else if (OVRInput.GetDown(_keyConfig.resize_D))
            {
                _height -= 0.05f;
                SetCameraHeight(_height);
            }

            _handMenu.UpdateLookat(_lookTarget);
        }

        public bool IsShowAny()
        {
            return _handMenu.IsShow;
        }
    }
}