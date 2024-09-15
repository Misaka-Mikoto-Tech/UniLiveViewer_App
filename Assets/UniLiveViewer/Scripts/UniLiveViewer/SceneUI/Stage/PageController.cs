using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer.Stage;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class PageController : MonoBehaviour
    {
        public Button_Switch[] BtnTab => _btnTab;
        [SerializeField] Button_Switch[] _btnTab;
        public Transform[] GetPageAnchor => _pageAnchor;
        [SerializeField] Transform[] _pageAnchor;

        public IObservable<Unit> ChangePageAsObservable => _changePageStream;
        readonly Subject<Unit> _changePageStream = new();

        public int Current { get; private set; }
        public Transform GetCurrentPageAnchor() { return _pageAnchor[Current]; }

        RootAudioSourceService _rootAudioSourceService;
        CancellationToken _cancellationToken;


        void Awake()
        {
            // 一旦
            var lifetimeScope = LifetimeScope.Find<StageSceneLifetimeScope>();
            _rootAudioSourceService = lifetimeScope.Container.Resolve<RootAudioSourceService>();
        }

        void Start()
        {
            if (_btnTab.Length != _pageAnchor.Length)
            {
                Debug.LogError("Page and number of buttons do not match.");
                return;
            }
            
            for (int i = 0; i < _btnTab.Length; i++)
            {
                _btnTab[i].onTrigger += SwitchCurrent;
            }

            _cancellationToken = this.GetCancellationTokenOnDestroy();

            SwitchPages();
        }

        /// <summary>
        /// ページを切り替える
        /// </summary>
        /// <param name="btn"></param>
        void SwitchCurrent(Button_Base btn)
        {
            //タブのボタン状態を更新する
            for (int i = 0; i < _btnTab.Length; i++)
            {
                if (_btnTab[i] != btn) continue;
                Current = i;
                break;
            }
            _rootAudioSourceService.PlayOneShot(AudioSE.TabClick);
            SwitchPages();
        }

        /// <summary>
        /// カレントページに切り替える
        /// </summary>
        void SwitchPages()
        {
            //カレントページのタブボタンが非表示の場合別ページへ
            if (!_btnTab[Current].gameObject.activeSelf)
            {
                for (int i = 0; i < _btnTab.Length; i++)
                {
                    if (!_btnTab[i]) continue;
                    if (!_btnTab[i].gameObject.activeSelf) continue;
                    Current = i;
                    break;
                }
            }

            //ページ切り替え
            var isEnable = false;
            for (int i = 0; i < _btnTab.Length; i++)
            {
                if (!_btnTab[i]) continue;
                isEnable = Current == i;
                _btnTab[i].isEnable = isEnable;
                if (_pageAnchor[i].gameObject.activeSelf != isEnable) _pageAnchor[i].gameObject.SetActive(isEnable);
            }

            _changePageStream.OnNext(Unit.Default);

            UniTask.Void(async () =>
            {
                await UniTask.Delay(400, cancellationToken: _cancellationToken);
                _rootAudioSourceService.PlayOneShot(AudioSE.SpringMenuItem);
            });
        }

        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        void OnEnable()
        {
            SwitchPages();
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Current++;
                if (Current >= _pageAnchor.Length) Current = 0;
                _rootAudioSourceService.PlayOneShot(AudioSE.TabClick);
                SwitchPages();
            }
        }
    }
}

