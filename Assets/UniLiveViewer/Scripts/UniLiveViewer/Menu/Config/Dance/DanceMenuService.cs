using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu.Config.Dance
{
    public class DanceMenuService
    {
        readonly DanceMenuSettings _settings;
        readonly AudioSourceService _audioSourceService;

        [Inject]
        public DanceMenuService(
            DanceMenuSettings settings,
            AudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        public void Initialize()
        {
            _settings.VMDSmoothButton.isEnable = FileReadAndWriteUtility.UserProfile.IsSmoothVMD;
            _settings.VMDSmoothButton.onTrigger += OnChangeVMDSmooth;

            _settings.VMDScaleSlider.Value = FileReadAndWriteUtility.UserProfile.VMDScale;
            _settings.VMDScaleSlider.ValueUpdate += () => OnUpdateVMDScale();
            _settings.VMDScaleSlider.UnControled += () => OnUnControledVMDScale();

            //初期化で一度だけ実行しておく
            OnUpdateVMDScale();
        }

        void OnChangeVMDSmooth(Button_Base button_Base)
        {
            FileReadAndWriteUtility.UserProfile.IsSmoothVMD = _settings.VMDSmoothButton.isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            _audioSourceService.PlayOneShot(0);
        }

        void OnUpdateVMDScale()
        {
            _settings.VMDScaleSlider.Value = Mathf.Clamp(_settings.VMDScaleSlider.Value, 0.3f, 1.0f);
            _settings.VMDScaleText.text = $"{_settings.VMDScaleSlider.Value:0.000}";
        }

        void OnUnControledVMDScale()
        {
            FileReadAndWriteUtility.UserProfile.VMDScale = float.Parse(_settings.VMDScaleSlider.Value.ToString("f3"));
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }
    }
}