using UnityEngine;

namespace UniLiveViewer
{
    public class LookAtLight : StageCharaObserver
    {
        [SerializeField] private Transform[] lights;
        private Vector3 distance;

        protected override void Init()
        {
            base.Init();

            for (int i = 0; i < targets.Length; i++)
            {
                if (lights[i].gameObject.activeSelf == targets[i]) continue;
                lights[i].gameObject.SetActive(targets[i]);
            }

            switch (targetList.Count)
            {
                case 1:
                    for (int i = 0; i < lights.Length; i++)
                    {
                        if (i == 0 || i == 3 || i == 4)
                        {
                            if (!lights[i].gameObject.activeSelf) lights[i].gameObject.SetActive(true);
                        }
                        else
                        {
                            if (lights[i].gameObject.activeSelf) lights[i].gameObject.SetActive(false);
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < lights.Length; i++)
                    {
                        if (i == 0)
                        {
                            if (lights[i].gameObject.activeSelf) lights[i].gameObject.SetActive(false);
                        }
                        else
                        {
                            if (!lights[i].gameObject.activeSelf) lights[i].gameObject.SetActive(true);
                        }
                    }
                    break;
                default:
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
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (targetList.Count)
            {
                case 1:
                    for (int i = 0; i < lights.Length; i++)
                    {
                        if (i == 0 || i == 3 || i == 4)
                        {
                            distance = (targetList[0].position + (targetList[0].forward * i * 0.05f))  - lights[i].position;
                            lights[i].up = distance;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < lights.Length; i++)
                    {
                        if (i == 0 || i == 4)
                        {
                            distance = (targetList[0].position + (targetList[0].forward * i * 0.05f)) - lights[i].position;
                            lights[i].up = distance;
                        }
                        else if (i == 1 || i == 2)
                        {
                            distance = (targetList[1].position + (targetList[0].forward * i * 0.05f)) - lights[i].position;
                            lights[i].up = distance;
                        }
                    }
                    break;
                default:
                    for (int i = 0; i < targetList.Count; i++)
                    {
                        distance = targetList[i].position - lights[i].position;
                        lights[i].up = distance;
                    }
                    break;
            }
        }
    }
}