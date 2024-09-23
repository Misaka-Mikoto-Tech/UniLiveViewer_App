using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UniLiveViewer
{
    public class SystemSettingsService
    {
        /// <summary>
        /// MEMO: Root側で早いのでReactiveしかないskipもNG
        /// </summary>
        public IReadOnlyReactiveProperty<SystemLanguage> SystemLanguage => _systemLanguage;
        readonly ReactiveProperty<SystemLanguage> _systemLanguage = new();

        public SystemSettingsService()
        {
        }

        /// <summary>
        /// API利用なのでIStartable以降
        /// </summary>
        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            //一応待つ
            await LocalizationSettings.InitializationOperation.Task.AsUniTask().AttachExternalCancellation(cancellation);

            if (FileReadAndWriteUtility.UserProfile.LanguageCode == -1)
            {
                Change(Application.systemLanguage);
            }
            else
            {
                _systemLanguage.Value = (SystemLanguage)FileReadAndWriteUtility.UserProfile.LanguageCode;
            }
        }

        public void Change(SystemLanguage systemLanguage)
        {
            var result = systemLanguage.CheckFallback();
            var locale = LocalizationSettings.AvailableLocales.GetLocale(result);
            LocalizationSettings.SelectedLocale = locale;

            FileReadAndWriteUtility.UserProfile.LanguageCode = (int)result;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _systemLanguage.Value = (SystemLanguage)FileReadAndWriteUtility.UserProfile.LanguageCode;
        }

    }
}