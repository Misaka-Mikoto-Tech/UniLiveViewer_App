using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Player
{
    public class GraphicsSettingsService
    {
        UniversalAdditionalCameraData _cameraData;
        bool _cachePostProcessing;
        Bloom _bloom;
        DepthOfField _depthOfField;
        Tonemapping _tonemapping;
        Vignette _vignette;

        readonly Camera _camera;
        readonly VolumeProfile _volumeProfile;

        [Inject]
        public GraphicsSettingsService(Camera camera, VolumeProfile volumeProfile)
        {
            _camera = camera;
            _volumeProfile = volumeProfile;
        }

        public void Initialize()
        {
            _cameraData = _camera.GetComponent<UniversalAdditionalCameraData>();
            _cachePostProcessing = _cameraData.renderPostProcessing;
            _cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

            if (_volumeProfile.TryGet<Bloom>(out var bloom))
            {
                _bloom = bloom;
                _bloom.active = false;
            }
            if (_volumeProfile.TryGet<DepthOfField>(out var depthOfField))
            {
                _depthOfField = depthOfField;
                _depthOfField.active = false;
            }
            if (_volumeProfile.TryGet<Tonemapping>(out var tonemapping))
            {
                _tonemapping = tonemapping;
                _tonemapping.active = false;
            }
            if (_volumeProfile.TryGet<Vignette>(out var vignette))
            {
                _vignette = vignette;
                _vignette.active = false;
            }
            IfNeededSwitchPostprocessing();
        }

        public void ChangeAntialiasing(AntialiasingMode mode)
        {
            if (mode == AntialiasingMode.SubpixelMorphologicalAntiAliasing) return;
            _cameraData.antialiasing = mode;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeBloom(bool isEnable)
        {
            _bloom.active = isEnable;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeDepthOfField(bool isEnable)
        {
            _depthOfField.active = isEnable;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeTonemapping(bool isEnable)
        {
            _tonemapping.active = isEnable;
            IfNeededSwitchPostprocessing();
        }

        public void ChangeVignette(bool isEnable)
        {
            _vignette.active = isEnable;
            IfNeededSwitchPostprocessing();
        }

        public void OnChangePassthrough(bool isEnablePassthrough)
        {
            if (isEnablePassthrough) ForceChangePostprocessing(false);
            else ResetPostProcessing();
        }

        void ForceChangePostprocessing(bool isEnable)
        {
            _cachePostProcessing = _cameraData.renderPostProcessing;
            _cameraData.renderPostProcessing = isEnable;
        }

        void ResetPostProcessing()
        {
            _cameraData.renderPostProcessing = _cachePostProcessing;
        }

        void IfNeededSwitchPostprocessing()
        {
            var isEnable = false;

            if(_cameraData.antialiasing != AntialiasingMode.None) isEnable = true;
            if (_bloom.active) isEnable = true;
            if (_depthOfField.active) isEnable = true;
            if (_tonemapping.active) isEnable = true;
            if (_vignette.active) isEnable = true;

            _cameraData.renderPostProcessing = isEnable;
        }
    }
}
