using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class TitleBackGroundService
    {
        readonly SpriteRendererSwitcher _spriteSwitcher;
        readonly TextMesh _textAppVersion;

        [Inject]
        TitleBackGroundService(
            SpriteRendererSwitcher spriteRendererSwitcher,
            TextMesh textAppVersion)
        {
            _spriteSwitcher = spriteRendererSwitcher;
            _textAppVersion = textAppVersion;

            _textAppVersion.text = "ver." + Application.version;
        }

        public void OnChangeLanguage(string sceneName)
        {
            var code = sceneName.Contains("_JP") ? 1 : 0;
            _spriteSwitcher.SetSprite(code);
        }
    }
}