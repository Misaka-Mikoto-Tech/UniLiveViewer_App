using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UniLiveViewer
{
    public class SystemSettingsService
    {
        public IReadOnlyReactiveProperty<int> LanguageIndex => _languageIndex;
        readonly ReactiveProperty<int> _languageIndex = new();

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
                _languageIndex.Value = GetLanguageIndex();
            }
        }

        public void Change(SystemLanguage systemLanguage)
        {
            var result = systemLanguage.CheckFallback();
            var locale = LocalizationSettings.AvailableLocales.GetLocale(result);
            LocalizationSettings.SelectedLocale = locale;

            FileReadAndWriteUtility.UserProfile.LanguageCode = (int)systemLanguage;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _languageIndex.Value = GetLanguageIndex();
        }

        int GetLanguageIndex()
        {
            var SystemLanguage = (SystemLanguage)FileReadAndWriteUtility.UserProfile.LanguageCode;
            return SystemLanguage.ToResourceIndex();
        }
    }
}