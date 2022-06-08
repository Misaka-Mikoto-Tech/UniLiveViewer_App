using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class LightBase : StageCharaObserver
    {
        [SerializeField] protected MeshRenderer[] lights;
        [SerializeField] protected AnimationCurve collarCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] protected AnimationCurve collarCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] protected AnimationCurve collarCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] protected float colorSpeed = 1;
        [SerializeField] protected string propertyName = "_TintColor";
        protected bool isWhitelight = true;
        protected float colorTimer = 0;


        public void SetLightCollar(bool isWhite)
        {
            isWhitelight = isWhite;
            if (isWhitelight)
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].sharedMaterial.SetColor(propertyName, Color.white);
                    lights[i].sharedMaterial.color = Color.white;
                }
            }
        }

        protected virtual void Update()
        {
            if (!isWhitelight)
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    lights[i].sharedMaterial.SetColor
                        (propertyName,
                        new Color(
                            collarCurveR.Evaluate(colorTimer),
                            collarCurveG.Evaluate(colorTimer),
                            collarCurveB.Evaluate(colorTimer)
                            )
                        );
                }
                colorTimer += Time.deltaTime * colorSpeed;
                if (colorTimer > 1.05f) colorTimer = 0;
            } 
        }
    }
}