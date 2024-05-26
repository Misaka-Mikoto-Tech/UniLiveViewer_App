using UnityEngine;

namespace UniLiveViewer
{
    public static class SystemLanguageExtensions
    {
        public static int ToResourceIndex(this SystemLanguage systemLanguage)
        {
            return systemLanguage switch
            {
                SystemLanguage.English => 0,
                SystemLanguage.Japanese => 1,
                _ => 0,
            };
        }

        public static SystemLanguage CheckFallback(this SystemLanguage systemLanguage)
        {
            return systemLanguage switch
            {
                SystemLanguage.English => SystemLanguage.English,
                SystemLanguage.Japanese => SystemLanguage.Japanese,
                _ => SystemLanguage.English,
            };
        }
    }
}