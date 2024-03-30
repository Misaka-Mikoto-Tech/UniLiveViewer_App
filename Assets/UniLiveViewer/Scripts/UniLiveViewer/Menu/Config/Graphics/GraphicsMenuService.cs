using UniRx;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuService
    {
        const string Edge = "_Edge";

        public IReadOnlyReactiveProperty<AntialiasingMode> AntialiasingMode => _antialiasingMode;
        ReactiveProperty<AntialiasingMode> _antialiasingMode = new((AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing);

        public IReadOnlyReactiveProperty<bool> Bloom => _bloom;
        ReactiveProperty<bool> _bloom = new(FileReadAndWriteUtility.UserProfile.IsBloom);

        public IReadOnlyReactiveProperty<bool> DepthOfField => _depthOfField;
        ReactiveProperty<bool> _depthOfField = new(FileReadAndWriteUtility.UserProfile.IsDepthOfField);

        public IReadOnlyReactiveProperty<bool> Tonemapping => _tonemapping;
        ReactiveProperty<bool> _tonemapping = new(FileReadAndWriteUtility.UserProfile.IsTonemapping);

        public IReadOnlyReactiveProperty<float> BloomThreshold => _bloomThreshold;
        ReactiveProperty<float> _bloomThreshold = new(FileReadAndWriteUtility.UserProfile.BloomThreshold);

        public IReadOnlyReactiveProperty<float> BloomIntensity => _bloomIntensity;
        ReactiveProperty<float> _bloomIntensity = new(FileReadAndWriteUtility.UserProfile.BloomIntensity);

        readonly AudioSourceService _audioSourceService;
        readonly GraphicsMenuSettings _settings;

        [Inject]
        public GraphicsMenuService(
            AudioSourceService audioSourceService,
            GraphicsMenuSettings settings)
        {
            _audioSourceService = audioSourceService;
            _settings = settings;
        }

        public void Initialize()
        {
            _settings.GraphicButton[0].isEnable = _antialiasingMode.Value != UnityEngine.Rendering.Universal.AntialiasingMode.None;
            _settings.GraphicButton[1].isEnable = _bloom.Value;
            _settings.GraphicButton[2].isEnable = _depthOfField.Value;
            _settings.GraphicButton[3].isEnable = _tonemapping.Value;
            foreach (var button in _settings.GraphicButton)
            {
                button.onTrigger += OnClick;
            }

            _settings.GraphicSlider[0].Value = _bloomThreshold.Value;
            _settings.GraphicSlider[1].Value = _bloomIntensity.Value;
            _settings.GraphicSlider[0].ValueUpdate += () => _bloomThreshold.Value = _settings.GraphicSlider[0].Value;
            _settings.GraphicSlider[1].ValueUpdate += () => _bloomIntensity.Value = _settings.GraphicSlider[1].Value;

            _settings.OutlineSlider.Value = 0;
            _settings.OutlineSlider.ValueUpdate += () => OnChangeOutline();

            _settings.OutlineMat.SetFloat(Edge, _settings.OutlineSlider.Value);
        }

        void OnClick(Button_Base btn)
        {
            if (btn == _settings.GraphicButton[0])
            {
                var mode = btn.isEnable ?
                    UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing :
                    UnityEngine.Rendering.Universal.AntialiasingMode.None;
                _antialiasingMode.Value = mode;
            }
            else if (btn == _settings.GraphicButton[1])
            {
                _bloom.Value = btn.isEnable;
            }
            else if (btn == _settings.GraphicButton[2])
            {
                _depthOfField.Value = btn.isEnable;
            }
            else if (btn == _settings.GraphicButton[3])
            {
                _tonemapping.Value = btn.isEnable;
            }
            _audioSourceService.PlayOneShot(0);
        }

        void OnChangeOutline()
        {
            if (0 < _settings.OutlineSlider.Value)
            {
                _settings.OutlineRender.SetActive(true);
                _settings.OutlineMat.SetFloat(Edge, _settings.OutlineSlider.Value);
            }
            else _settings.OutlineRender.SetActive(false);
        }
    }
}