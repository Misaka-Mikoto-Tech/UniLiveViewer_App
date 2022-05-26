using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class AutoCamera : MonoBehaviour
    {
        enum SWITCHTYPE
        {
            ALL,
            RANDOM_ONE,
            RANDOM_BACKIMAGE
        }

        private Transform target;
        public bool isUpdate = true;
        [SerializeField] private int interval = 5000;
        [SerializeField] private SWITCHTYPE switchType = SWITCHTYPE.ALL;//カメラ候補を切り替えるモード
        [SerializeField] private Camera[] _camera;
        [SerializeField] private SpriteRenderer[] _spr;
        private CancellationToken cancellationToken;
        private TimelineController timeline;

        // Start is called before the first frame update
        void Start()
        {
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            cancellationToken = this.GetCancellationTokenOnDestroy();

            timeline.FieldCharaAdded += Init;
            timeline.FieldCharaDeleted += Init;

            foreach (var e in _camera)
            {
                e.gameObject.SetActive(false);
            }

            AutoUpdate().Forget();
        }

        private void Init()
        {
            if (target) return;
            for (int i = 0;i< timeline.trackBindChara.Length;i++)
            {
                if (i == TimelineController.PORTAL_ELEMENT) continue;
                if (timeline.trackBindChara[i])
                {
                    target = timeline.trackBindChara[i].lookAtCon.virtualChest;
                    break;
                }
            }
        }

        private async UniTask AutoUpdate()
        {
            int r = 0;
            Vector3 pos;

            while (true)
            {
                await UniTask.Delay(interval, cancellationToken:cancellationToken);
                if (!isUpdate) continue;

                if(target)
                {
                    pos = target.position + (target.forward * 2);
                    pos += (target.up * Random.Range(-0.1f, 0.25f)) + (target.right * Random.Range(-0.2f, 0.2f));
                    foreach (var e in _camera)
                    {
                        e.transform.position = pos;
                        e.transform.forward = target.position - e.transform.position;
                    }
                }

                //スクリーンに一瞬反映させる
                switch (switchType)
                {
                    case SWITCHTYPE.ALL:
                        foreach (var e in _camera) e.gameObject.SetActive(true);
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                        foreach (var e in _camera) e.gameObject.SetActive(false);
                        break;
                    case SWITCHTYPE.RANDOM_ONE:
                        r = Random.Range(0, _camera.Length);
                        _camera[r].gameObject.SetActive(true);
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                        _camera[r].gameObject.SetActive(false);
                        break;
                    case SWITCHTYPE.RANDOM_BACKIMAGE:
                        r = Random.Range(0, _spr.Length);
                        for (int i = 0;i< _spr.Length;i++) _spr[i].enabled = (i == r);
                        foreach (var e in _camera) e.gameObject.SetActive(true);
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                        foreach (var e in _camera) e.gameObject.SetActive(false);
                        break;
                }
            }
        }
    }
}