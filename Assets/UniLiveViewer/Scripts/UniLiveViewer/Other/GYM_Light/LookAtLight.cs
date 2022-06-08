using UnityEngine;

namespace UniLiveViewer
{
    public class LookAtLight : LightBase
    {
        private Vector3 distance;

        protected override void Init()
        {
            base.Init();

            for (int i = 0; i < targets.Length; i++)
            {
                if (lights[i].gameObject.activeSelf == targets[i]) continue;
                lights[i].gameObject.SetActive(targets[i]);
            }

            for (int i = 0; i < lights.Length; i++)
            {
                if (i < targetList.Count)
                {
                    if (!lights[i].gameObject.activeSelf) lights[i].gameObject.SetActive(true);
                }
                else
                {
                    if (lights[i].gameObject.activeSelf) lights[i].gameObject.SetActive(false);
                }
            }
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            for (int i = 0; i < targetList.Count; i++)
            {
                distance = targetList[i].position - lights[i].transform.position;
                lights[i].transform.up = distance;
            }
        }
    }
}