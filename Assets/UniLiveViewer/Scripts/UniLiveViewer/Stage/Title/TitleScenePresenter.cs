using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Title
{
    public class TitleScenePresenter : IStartable
    {
        readonly TitleSceneSettings _titleSceneSettings;

        [Inject]
        public TitleScenePresenter(TitleSceneSettings titleSceneSettings)
        {
            _titleSceneSettings = titleSceneSettings;
        }

        void IStartable.Start()
        {
            _titleSceneSettings.AppVersionText.text = "ver." + Application.version;
        }
    }
}