using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class AutoCamera : MonoBehaviour
    {
        public bool isUpdate = true;
        public bool isSwitchingMode = false;//カメラ候補を切り替えるモード
        [SerializeField] private Camera[] _camera = new Camera[2];

        // Start is called before the first frame update
        void Start()
        {
            _camera[0].enabled = false;
            _camera[1].enabled = false;

            if (isSwitchingMode)
            {
                StartCoroutine(SwitchingUpdate());
            }
            else
            {
                StartCoroutine(AutoUpdate());
            }
        }

        private IEnumerator AutoUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(4f);

                if (isUpdate)
                {
                    //スクリーンに一瞬反映させる
                    _camera[0].enabled = true;
                    _camera[1].enabled = true;
                    yield return null;
                    _camera[0].enabled = false;
                    _camera[1].enabled = false;
                }
            }
        }
        private IEnumerator SwitchingUpdate()
        {
            while (_camera.Length > 0)
            {
                foreach (var e in _camera)
                {
                    if (isUpdate)
                    {
                        e.enabled = true;
                        yield return null;
                        e.enabled = false;
                    }
                    yield return new WaitForSeconds(4f);
                }
            }
        }
    }
}