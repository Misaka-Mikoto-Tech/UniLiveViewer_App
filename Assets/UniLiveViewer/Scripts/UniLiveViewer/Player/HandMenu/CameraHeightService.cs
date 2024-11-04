using UniLiveViewer.OVRCustom;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Player.HandMenu
{
    public class CameraHeightService
    {
        UniLiveViewer.HandMenu _handMenu;

        float _height;
        bool _isSetupComplete;

        readonly CharacterCameraConstraint_Custom _characterCameraConstraintCustom;
        readonly PlayerHandMenuAnchorL _playerHandMenuAnchorL;
        readonly PlayerHandMenuSettings _playerHandMenuSettings;
        readonly Transform _lookTarget;
        readonly RootAudioSourceService _audioSourceService;

        [Inject]
        public CameraHeightService(
            CharacterCameraConstraint_Custom characterCameraConstraintCustom,
            PlayerHandMenuAnchorL playerHandMenuAnchorL,
            PlayerHandMenuSettings playerHandMenuSettings,
            Camera camera,
            RootAudioSourceService audioSourceService)
        {
            _characterCameraConstraintCustom = characterCameraConstraintCustom;
            _playerHandMenuAnchorL = playerHandMenuAnchorL;
            _playerHandMenuSettings = playerHandMenuSettings;
            _lookTarget = camera.transform;
            _audioSourceService = audioSourceService;
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

            if (_handMenu.IsShow)
            {
                _audioSourceService.PlayOneShot(AudioSE.MenuOpen);
            }
            else
            {
                _audioSourceService.PlayOneShot(AudioSE.MenuClose);
            }
        }

        public void OnClickStickUp()
        {
            if (!_isSetupComplete) return;
            if (!_handMenu.IsShow) return;

            //Playerカメラの高さ調整
            _height += 0.05f;
            SetCameraHeight(_height);
        }

        public void OnClickStickDown()
        {
            if (!_isSetupComplete) return;
            if (!_handMenu.IsShow) return;

            //Playerカメラの高さ調整
            _height -= 0.05f;
            SetCameraHeight(_height);
        }

        public void OnLateTick()
        {
            if (!_isSetupComplete) return;
            if (!_handMenu.IsShow) return;

            _handMenu.UpdateLookat(_lookTarget);
        }

        public bool IsShowAny()
        {
            return _handMenu.IsShow;
        }
    }
}