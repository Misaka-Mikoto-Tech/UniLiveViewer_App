using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UniLiveViewer
{
    public class SystemSettingsService
    {
        public IObservable<int> LanguageIndexAsObservable => _languageIndexStream;
        readonly Subject<int> _languageIndexStream = new();

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
                _languageIndexStream.OnNext(GetLanguageIndex());
            }
        }

        public void Change(SystemLanguage systemLanguage)
        {
            var result = systemLanguage.CheckFallback();
            var locale = LocalizationSettings.AvailableLocales.GetLocale(result);
            LocalizationSettings.SelectedLocale = locale;

            FileReadAndWriteUtility.UserProfile.LanguageCode = (int)systemLanguage;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            _languageIndexStream.OnNext(GetLanguageIndex());
        }

        int GetLanguageIndex()
        {
            var systemLanguage = (SystemLanguage)FileReadAndWriteUtility.UserProfile.LanguageCode;
            return systemLanguage.ToResourceIndex();
        }
    }
}