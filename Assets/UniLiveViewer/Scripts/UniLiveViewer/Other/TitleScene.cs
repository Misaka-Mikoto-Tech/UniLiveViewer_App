using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;

namespace UniLiveViewer 
{
    public class TitleScene : MonoBehaviour
    {
        [SerializeField] SpriteRendererSwitcher _spriteRendererSwitcher;
        [SerializeField] TextMesh _appVersion;
        [SerializeField] Transform _uiRoot;

        // TODO: 言語切り替えボタン、もっとカッコよくしたい手とかイランだろ
        [SerializeField] private Button_Base[] btn_Language = new Button_Base[2];
        [SerializeField] private GameObject manualHand;
        [SerializeField] private Transform[] Chara = new Transform[3];

        [SerializeField] AudioClip[] _sound;
        AudioSource _audioSource;
        CancellationToken _cancellationToken;

        public IObservable<string> ChangeSceneAsObservable => _changeSceneStream;
        Subject<string> _changeSceneStream;

        void Awake()
        {
            _appVersion.text = "ver." + Application.version;
            _cancellationToken = this.GetCancellationTokenOnDestroy();

            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;

            _changeSceneStream = new Subject<string>();

            //randomにキャラ変更
            int r = UnityEngine.Random.Range(0, 3);
            for (int i = 0; i < 3; i++)
            {
                if (r == i) Chara[i].gameObject.SetActive(true);
                else Chara[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < btn_Language.Length; i++)
            {
                btn_Language[i].onTrigger += onChangeLanguage;
            }
            manualHand.SetActive(false);

            _uiRoot.gameObject.SetActive(false);            
        }

        public void Begin()
        {
            if (SystemInfo.userProfile.LanguageCode != (int)USE_LANGUAGE.NULL)
            {
                //2回目以降
                _spriteRendererSwitcher.gameObject.SetActive(false);
                SceneChange().Forget();
            }
            else InitHand().Forget();
        }

        async UniTask InitHand()
        {
            await UniTask.Delay(2000, cancellationToken: _cancellationToken);
            _uiRoot.gameObject.SetActive(true);
            await UniTask.Delay(1000, cancellationToken: _cancellationToken);
            manualHand.SetActive(true);
        }

        void onChangeLanguage(Button_Base btn)
        {
            if (btn.name.Contains("_JP"))
            {
                SystemInfo.userProfile.LanguageCode = (int)USE_LANGUAGE.JP;
                FileReadAndWriteUtility.WriteJson(SystemInfo.userProfile);
                //差し替える
                _spriteRendererSwitcher.SetSprite(1);
            }
            else
            {
                SystemInfo.userProfile.LanguageCode = (int)USE_LANGUAGE.EN;
                FileReadAndWriteUtility.WriteJson(SystemInfo.userProfile);
                //差し替える
                _spriteRendererSwitcher.SetSprite(0);
            }

            //クリック音
            _audioSource.PlayOneShot(_sound[0]);
            //Handを消す
            manualHand.SetActive(false);

            SceneChange().Forget();
        }

        async UniTask SceneChange()
        {
            await UniTask.Delay(500, cancellationToken: _cancellationToken);
            if (_uiRoot.gameObject.activeSelf) _uiRoot.gameObject.SetActive(false);

            await UniTask.Delay(1000, cancellationToken: _cancellationToken);
            _changeSceneStream.OnNext(SystemInfo.userProfile.LastSceneName);
        }
    }
}
