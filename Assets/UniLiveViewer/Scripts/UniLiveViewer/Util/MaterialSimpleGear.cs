using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{
    public class MaterialSimpleGear : MonoBehaviour
    {
        public int materialIndex;
        public string targetName = "_Amplitude";
        public AnimationCurve floatCurve = AnimationCurve.Linear(0, 0, 1, 2);

        private Material material;

        private void Awake()
        {
            material = GetComponent<Renderer>().materials[materialIndex];
            StartCoroutine("UpdateMaterial");
        }

        private IEnumerator UpdateMaterial()
        {
            float t = 0;

            while (gameObject)
            {
                material.SetFloat(targetName, floatCurve.Evaluate(t));
                t += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }
}