using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuService
    {
        readonly CommonMenuSettings _settings;
        readonly PassthroughService _passthroughService;
        readonly AudioSourceService _audioSourceService;

        [Inject]
        public CommonMenuService(
            CommonMenuSettings settings,
            PassthroughService passthroughService,
            AudioSourceService audioSourceService)
        {
            _settings = settings;
            _passthroughService = passthroughService;
            _audioSourceService = audioSourceService;
        }

        public void Initialize()
        {
            _settings.PassthroughButton.isEnable = _passthroughService.IsInsightPassthroughEnabled();
            _settings.VibrationButton.isEnable = FileReadAndWriteUtility.UserProfile.TouchVibration;

            _settings.PassthroughButton.onTrigger += OnChangePassthrough;
            _settings.VibrationButton.onTrigger += OnChangeControllerVibration;
            _settings.FixedFoveatedSlider.ValueUpdate += () =>  OnUpdateFixedFoveated();

            //初期化で一度だけ実行しておく
            OnUpdateFixedFoveated();
        }

        void OnChangePassthrough(Button_Base button_Base)
        {
            _passthroughService.Switching(_settings.PassthroughButton.isEnable);
            _audioSourceService.PlayOneShot(0);
        }

        void OnChangeControllerVibration(Button_Base button_Base)
        {
            FileReadAndWriteUtility.UserProfile.TouchVibration = _settings.VibrationButton.isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            _audioSourceService.PlayOneShot(0);
        }

        /// <summary>
        /// 固定中心窩レンダリングのスライダー
        /// </summary>
        void OnUpdateFixedFoveated()
        {
            _settings.FixedFoveatedSlider.Value = Mathf.Clamp(_settings.FixedFoveatedSlider.Value, 2, 4);
#if UNITY_EDITOR
            _settings.FixedFoveatedText.text = $"noQuest:{_settings.FixedFoveatedSlider.Value}";
#elif UNITY_ANDROID
            OVRManager.fixedFoveatedRenderingLevel = (OVRManager.FixedFoveatedRenderingLevel)_settings.FixedFoveatedSlider.Value;
            _settings.FixedFoveatedText.text = Enum.GetName(typeof(OVRManager.FixedFoveatedRenderingLevel),OVRManager.fixedFoveatedRenderingLevel);
#endif
        }
    }
}