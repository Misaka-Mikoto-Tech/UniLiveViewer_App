using Cysharp.Threading.Tasks;
using UniLiveViewer.SceneLoader;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuGripperService
    {        
        Vector3 EndPoint = new Vector3(0, 0.7f, 5);
        Vector3 keepDistance;

        bool isInit = false;
        CompositeDisposable _disposables;

        readonly Transform _transform;
        readonly Renderer _renderer;

        [Inject]
        MenuGripperService(Transform transform, Renderer renderer)
        {
            _transform = transform;
            _renderer = renderer;
        }

        public void OnStart()
        {
            switch (SceneChangeService.GetSceneType)
            {
                case SceneType.CANDY_LIVE:
                    EndPoint = new Vector3(4, 1.0f, 5.5f);
                    _transform.position = EndPoint + (Vector3.up * 2);
                    _transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                    break;
                case SceneType.KAGURA_LIVE:
                    EndPoint = new Vector3(0, 1.35f, 3);
                    _transform.position = EndPoint + (Vector3.up * 2);
                    _transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneType.VIEWER:
                    EndPoint = new Vector3(0, 1.0f, 4);
                    _transform.position = EndPoint + (Vector3.up * 2);
                    _transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneType.GYMNASIUM:
                    EndPoint = new Vector3(0, 1.0f, 4);
                    _transform.position = EndPoint + (Vector3.up * 2);
                    _transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
            }
        }

        public void Initialize()
        {
            _transform.position = EndPoint;
            DelayUIOpen().Forget();
        }

        public void OnSwitchEnable(bool isEnable)
        {
            _renderer.enabled = isEnable;
        }

        /// <summary>
        /// 時間差でUIを表示する
        /// </summary>
        async UniTask DelayUIOpen()
        {
            //時間差でUIを表示する
            //SwitchEnable(false);
            //await UniTask.Delay(800);
            //SwitchEnable(true);
            //isInit = true;
        }

        //void Update()
        //{
        //    if (!isInit) return;
        //    guideTarget.position = guideParent.position;
        //    guideTarget.rotation = guideParent.rotation;
        //}
    }
}