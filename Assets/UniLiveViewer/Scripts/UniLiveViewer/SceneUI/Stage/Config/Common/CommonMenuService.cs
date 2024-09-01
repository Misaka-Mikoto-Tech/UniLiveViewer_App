using UniLiveViewer.Player;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuService
    {
        readonly SystemSettingsService _systemSettingsService;
        readonly CommonMenuSettings _settings;
        readonly PassthroughService _passthroughService;
        readonly RootAudioSourceService _audioSourceService;

        [Inject]
        public CommonMenuService(
            SystemSettingsService systemSettingsService,
            CommonMenuSettings settings,
            PassthroughService passthroughService,
            RootAudioSourceService audioSourceService)
        {
            _systemSettingsService = systemSettingsService;
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

            _settings.EnglishButton.onTrigger += (btn) =>
            {
                _systemSettingsService.Change(SystemLanguage.English);
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            };
            _settings.JapaneseButton.onTrigger += (btn) =>
            {
                _systemSettingsService.Change(SystemLanguage.Japanese);
                _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
            };
        }

        void OnChangePassthrough(Button_Base button_Base)
        {
            _passthroughService.Switching(_settings.PassthroughButton.isEnable);
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        void OnChangeControllerVibration(Button_Base button_Base)
        {
            FileReadAndWriteUtility.UserProfile.TouchVibration = _settings.VibrationButton.isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            _audioSourceService.PlayOneShot(AudioSE.ButtonClick);
        }

        /// <summary>
        /// 固定中心窩レンダリングのスライダー
        /// </summary>
        public void OnUpdateFixedFoveated(float value)
        {
#if UNITY_EDITOR
            _settings.FixedFoveatedText.text = $"noQuest:{value}";
#elif UNITY_ANDROID
            OVRManager.fixedFoveatedRenderingLevel = (OVRManager.FixedFoveatedRenderingLevel)value;
            _settings.FixedFoveatedText.text = System.Enum.GetName(typeof(OVRManager.FixedFoveatedRenderingLevel),OVRManager.fixedFoveatedRenderingLevel);
#endif
        }
    }
}