using System;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuSettings : MonoBehaviour
    {
        [SerializeField] Button_Switch[] _sceneButton;

        public IObservable<SceneType> ChangeSceneAsObservable => _stream;
        readonly Subject<SceneType> _stream = new();

        void Start()
        {
            foreach (var button in _sceneButton)
            {
                button.isEnable = false;
            }

            // Button_Base改修するまでの繋ぎ
            _sceneButton[0].onTrigger += (btn) => _stream.OnNext(SceneType.TITLE);
            _sceneButton[1].onTrigger += (btn) => _stream.OnNext(SceneType.CANDY_LIVE);
            _sceneButton[2].onTrigger += (btn) => _stream.OnNext(SceneType.KAGURA_LIVE);
            _sceneButton[3].onTrigger += (btn) => _stream.OnNext(SceneType.VIEWER);
            _sceneButton[4].onTrigger += (btn) => _stream.OnNext(SceneType.GYMNASIUM);
            _sceneButton[5].onTrigger += (btn) => _stream.OnNext(SceneType.FANTASY_VILLAGE);
        }
    }
}
