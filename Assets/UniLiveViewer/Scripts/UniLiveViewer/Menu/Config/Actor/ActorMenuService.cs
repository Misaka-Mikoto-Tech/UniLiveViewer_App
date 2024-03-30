using UniLiveViewer.Timeline;
using VContainer;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuService
    {
        readonly ActorMenuSettings _settings;
        readonly QuasiShadowSetting _quasiShadowSetting;
        readonly AudioSourceService _audioSourceService;

        [Inject]
        public ActorMenuService(
            ActorMenuSettings settings,
            QuasiShadowSetting quasiShadowSetting,
            AudioSourceService audioSourceService)
        {
            _settings = settings;
            _quasiShadowSetting = quasiShadowSetting;
            _audioSourceService = audioSourceService;
        }

        public void Initialize()
        {
            _settings.InitialActorSizeSlider.Value = FileReadAndWriteUtility.UserProfile.InitCharaSize;
            _settings.InitialActorSizeSlider.ValueUpdate += () => OnUpdateActorSize();
            _settings.InitialActorSizeSlider.UnControled += () => OnUnControledActorSize();

            _settings.FallingShadowText.text = $"FootShadow:\n{_quasiShadowSetting.ShadowType}";
            _settings.FallingShadowLButton.onTrigger += OnChangeFallingShadowL;
            _settings.FallingShadowRButton.onTrigger += OnChangeFallingShadowR;
            _settings.FallingShadowSlider.Value = _quasiShadowSetting.ShadowScale;
            _settings.FallingShadowSlider.ValueUpdate += () => OnUpdateFallingShadow();
            _settings.FallingShadowSlider.UnControled += () => OnUnControledFallingShadow();

            //初期化で一度だけ実行しておく
            OnUpdateActorSize();
            OnUpdateFallingShadow();
        }

        void OnUpdateActorSize()
        {
            _settings.InitialActorSizeText.text = $"{_settings.InitialActorSizeSlider.Value:0.00}";
        }

        void OnUnControledActorSize()
        {
            FileReadAndWriteUtility.UserProfile.CharaShadow = float.Parse(_settings.InitialActorSizeSlider.Value.ToString("f2"));
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        void OnChangeFallingShadowL(Button_Base button_Base)
        {
            OnChangeFallingShadow(-1);
        }

        void OnChangeFallingShadowR(Button_Base button_Base)
        {
            OnChangeFallingShadow(1);
        }

        void OnChangeFallingShadow(int add)
        {
            _quasiShadowSetting.ShadowType += add;
            _settings.FallingShadowText.text = $"FootShadow:\n{_quasiShadowSetting.ShadowType}";
            FileReadAndWriteUtility.UserProfile.CharaShadowType = (int)_quasiShadowSetting.ShadowType;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _audioSourceService.PlayOneShot(0);
        }

        void OnUpdateFallingShadow()
        {
            var value = _settings.FallingShadowSlider.Value;
            _quasiShadowSetting.SetShadowScale(value);
            _settings.FallingShadowText.text = $"{value:0.00}";
        }

        void OnUnControledFallingShadow()
        {
            FileReadAndWriteUtility.UserProfile.CharaShadow = float.Parse(_settings.FallingShadowSlider.Value.ToString("f2"));
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }
    }
}