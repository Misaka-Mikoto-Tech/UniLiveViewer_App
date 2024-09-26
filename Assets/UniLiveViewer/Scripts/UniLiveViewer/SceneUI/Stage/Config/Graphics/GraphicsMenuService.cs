using System;
using UniRx;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuService: IDisposable
    {
        const string Edge = "_Edge";

        public IReadOnlyReactiveProperty<float> LightIntensity => _lightIntensity;
        readonly ReactiveProperty<float> _lightIntensity = new(1);

        public IReadOnlyReactiveProperty<AntialiasingMode> AntialiasingMode => _antialiasingMode;
        readonly ReactiveProperty<AntialiasingMode> _antialiasingMode = new((AntialiasingMode)FileReadAndWriteUtility.UserProfile.Antialiasing);

        public IReadOnlyReactiveProperty<bool> Bloom => _bloom;
        readonly ReactiveProperty<bool> _bloom = new(FileReadAndWriteUtility.UserProfile.IsBloom);

        public IReadOnlyReactiveProperty<bool> DepthOfField => _depthOfField;
        readonly ReactiveProperty<bool> _depthOfField = new(FileReadAndWriteUtility.UserProfile.IsDepthOfField);

        public IReadOnlyReactiveProperty<bool> Tonemapping => _tonemapping;
        readonly ReactiveProperty<bool> _tonemapping = new(FileReadAndWriteUtility.UserProfile.IsTonemapping);

        public IReadOnlyReactiveProperty<float> BloomThreshold => _bloomThreshold;
        readonly ReactiveProperty<float> _bloomThreshold = new(FileReadAndWriteUtility.UserProfile.BloomThreshold);

        public IReadOnlyReactiveProperty<float> BloomIntensity => _bloomIntensity;
        readonly ReactiveProperty<float> _bloomIntensity = new(FileReadAndWriteUtility.UserProfile.BloomIntensity);
        public IReadOnlyReactiveProperty<float> BloomColor => _bloomColor;
        readonly ReactiveProperty<float> _bloomColor = new(0.65f);//水色

        readonly RootAudioSourceService _audioSourceService;
        readonly GraphicsMenuSettings _settings;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GraphicsMenuService(
            RootAudioSourceService audioSourceService,
            GraphicsMenuSettings settings)
        {
            _audioSourceService = audioSourceService;
            _settings = settings;
        }

        public void Initialize()
        {
            // 購読前に初期化
            {
                _settings.GraphicsText[0].text = $"{_lightIntensity.Value:0.00}";

                _settings.GraphicsText[1].text = $"{_bloomThreshold.Value:0.00}";

                _settings.GraphicsText[2].text = $"{_bloomIntensity.Value:0.0}";

                _settings.GraphicsText[3].text = $"{0:0.00}";
            }


            _settings.GraphicButton[0].isEnable = _antialiasingMode.Value != UnityEngine.Rendering.Universal.AntialiasingMode.None;
            _settings.GraphicButton[1].isEnable = _bloom.Value;
            _settings.GraphicButton[2].isEnable = _depthOfField.Value;
            _settings.GraphicButton[3].isEnable = _tonemapping.Value;
            foreach (var button in _settings.GraphicButton)
            {
                button.onTrigger += OnClick;
            }

            _settings.GraphicSlider[0].ValueAsObservable
                .Subscribe(x => 
                {
                    _settings.GraphicsText[0].text = $"{x:0.00}";
                    _lightIntensity.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[1].ValueAsObservable
                .Subscribe(x => 
                {
                    _settings.GraphicsText[1].text = $"{x:0.00}";
                    _bloomThreshold.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[2].ValueAsObservable
                .Subscribe(x => 
                {
                    _settings.GraphicsText[2].text = $"{x:0.0}";
                    _bloomIntensity.Value = x;
                }).AddTo(_disposables);
            _settings.GraphicSlider[3].ValueAsObservable
                .Subscribe(x => _bloomColor.Value = x).AddTo(_disposables);
            _settings.OutlineSlider.ValueAsObservable
                .Subscribe(OnChangeOutline).AddTo(_disposables);

            _settings.GraphicSlider[0].Value = 1;
            _settings.GraphicSlider[1].Value = _bloomThreshold.Value;
            _settings.GraphicSlider[2].Value = _bloomIntensity.Value;
            _settings.GraphicSlider[3].Value = _bloomColor.Value;
            _settings.OutlineSlider.Value = 0;
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
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void OnChangeOutline(float value)
        {
            _settings.GraphicsText[3].text = $"{value:0.00}";

            if (_settings.OutlineRender == null) return;

            if (0 < value)
            {
                _settings.OutlineRender.SetActive(true);
                _settings.OutlineMat.SetFloat(Edge, value);
            }
            else _settings.OutlineRender.SetActive(false);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}