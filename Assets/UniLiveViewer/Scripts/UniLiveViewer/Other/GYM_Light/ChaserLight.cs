using UnityEngine;

namespace UniLiveViewer
{
    public class ChaserLight : LightBase
    {
        private Vector3 pos;

        protected override void Init()
        {
            base.Init();

            for (int i = 0; i < lights.Length; i++)
            {
                if(i >= targets.Length) lights[i].gameObject.SetActive(false);
                else if (lights[i].gameObject.activeSelf != targets[i]) lights[i].gameObject.SetActive(targets[i]);
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            for (int i = 0; i < targets.Length; i++)
            {
                if (!targets[i]) continue;
                pos = targets[i].position;
                pos.y = lights[i].transform.position.y;
                lights[i].transform.position = pos;
            }
        }
    }
}