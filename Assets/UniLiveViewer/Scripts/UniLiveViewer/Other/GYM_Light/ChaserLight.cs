using UnityEngine;

namespace UniLiveViewer
{
    public class ChaserLight : StageCharaObserver
    {
        [SerializeField] private Transform[] lights;
        private Vector3 pos;

        protected override void Init()
        {
            base.Init();

            for (int i = 0; i < targets.Length; i++)
            {
                if (lights[i].gameObject.activeSelf == targets[i]) continue;
                lights[i].gameObject.SetActive(targets[i]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;
                pos = targets[i].position;
                pos.y = lights[i].position.y;
                lights[i].position = pos;
            }
        }
    }
}