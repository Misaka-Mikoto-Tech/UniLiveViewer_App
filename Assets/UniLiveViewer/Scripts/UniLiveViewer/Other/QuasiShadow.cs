using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class QuasiShadow : MonoBehaviour
    {
        public enum SHADOWTYPE
        {
            NONE,
            CIRCLE,
            CIRCLE_TWIN,
            CROSS,
            CROSS_TWIN,
        }

        [SerializeField] private SpriteRenderer[] spritePrefab;
        public float shadowScale = 1.0f;
        private const float SINGLE_SHADOW = 0.2f;
        private const float TWIN_SHADOW = 0.1f;

        [Header("確認用露出(readonly)")]
        [SerializeField] private SHADOWTYPE shadowType = SHADOWTYPE.NONE;
        public SHADOWTYPE ShadowType
        { 
            get { return shadowType; } 
            set 
            { 
                shadowType = value;
                Update_ShadowSpriteRenderer();
            } 
        }
        private TimelineController timeline;
        private AnimatorData[] animators;

        private int typeLength = Enum.GetNames(typeof(SHADOWTYPE)).Length;

        // Start is called before the first frame update
        void Start()
        {
            timeline = GetComponent<TimelineController>();
            if(timeline)
            {
                timeline.FieldCharaAdded += Update_AnimatorData;
                timeline.FieldCharaDeleted += Update_AnimatorData;
            }

            animators = new AnimatorData[timeline.trackBindChara.Length];
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i] = new AnimatorData();
            }
        }

        public string GetTypeName(int moveIndex)
        {
            if ((int)shadowType + moveIndex >= typeLength) ShadowType = 0;
            else if ((int)shadowType + moveIndex < 0) ShadowType = (SHADOWTYPE)(typeLength - 1);
            else ShadowType += moveIndex;

            return shadowType.ToString();
        }

        private void Update_AnimatorData()
        {
            for (int i = 0;i< timeline.trackBindChara.Length;i++)
            {
                if (i == TimelineController.PORTAL_ELEMENT) continue;
                if (!timeline.trackBindChara[i]) animators[i].ClearAnimator();
                else animators[i].SetAnimator(timeline.trackBindChara[i].GetComponent<Animator>());
            }
        }

        private void Update_ShadowSpriteRenderer()
        {
            SpriteRenderer target = null;
            if (shadowType == SHADOWTYPE.CIRCLE || shadowType == SHADOWTYPE.CIRCLE_TWIN) target = spritePrefab[0];
            if (shadowType == SHADOWTYPE.CROSS || shadowType == SHADOWTYPE.CROSS_TWIN) target = spritePrefab[1];


            if (shadowType == SHADOWTYPE.CIRCLE || shadowType == SHADOWTYPE.CROSS)
            {
                for (int i = 0; i < timeline.trackBindChara.Length; i++)
                {
                    if (i == TimelineController.PORTAL_ELEMENT) continue;
                    animators[i].SetSpriteRenderer(Instantiate(target));
                }
            }
            else if (shadowType == SHADOWTYPE.CIRCLE_TWIN || shadowType == SHADOWTYPE.CROSS_TWIN)
            {
                for (int i = 0; i < timeline.trackBindChara.Length; i++)
                {
                    if (i == TimelineController.PORTAL_ELEMENT) continue;
                    animators[i].SetSpriteRenderer(Instantiate(target), Instantiate(target));
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 scale;

            if (shadowType == SHADOWTYPE.CIRCLE || shadowType == SHADOWTYPE.CROSS)
            {
                scale = Vector3.one * SINGLE_SHADOW * shadowScale;
                for (int i = 0; i < animators.Length; i++)
                {
                    if (!animators[i].anime) continue;
                    animators[i].spriteRenderers[0].transform.position = animators[i].anime.transform.position;
                    animators[i].spriteRenderers[0].transform.localScale = scale;
                }
            }
            else if (shadowType == SHADOWTYPE.CIRCLE_TWIN || shadowType == SHADOWTYPE.CROSS_TWIN)
            {
                Vector3 pos = Vector3.zero;
                scale = Vector3.one * TWIN_SHADOW * shadowScale;
                for (int i = 0; i < animators.Length; i++)
                {
                    if (!animators[i].anime) continue;

                    pos = animators[i].leftFoot.position;
                    pos.y = animators[i].anime.transform.position.y;
                    animators[i].spriteRenderers[0].transform.position = pos;
                    animators[i].spriteRenderers[0].transform.localScale = scale;

                    pos = animators[i].rightFoot.position;
                    pos.y = animators[i].anime.transform.position.y;
                    animators[i].spriteRenderers[1].transform.position = pos;
                    animators[i].spriteRenderers[1].transform.localScale = scale;
                }
            }
        }

        public class AnimatorData
        {
            public Animator anime;
            public Transform leftFoot;
            public Transform rightFoot;
            public SpriteRenderer[] spriteRenderers = new SpriteRenderer[2];

            public void SetAnimator(Animator _anime)
            {
                anime = _anime;
                leftFoot = anime.GetBoneTransform(HumanBodyBones.LeftFoot);
                rightFoot = anime.GetBoneTransform(HumanBodyBones.RightFoot);

                if (spriteRenderers[0]) spriteRenderers[0].enabled = true;
                if (spriteRenderers[1]) spriteRenderers[1].enabled = true;
            }
            public void ClearAnimator()
            {
                anime = null;
                leftFoot = null;
                rightFoot = null;

                if (spriteRenderers[0]) spriteRenderers[0].enabled = false;
                if (spriteRenderers[1]) spriteRenderers[1].enabled = false;
            }
            public void SetSpriteRenderer(SpriteRenderer single)
            {
                Dispose();
                spriteRenderers[0] = single;
                spriteRenderers[1] = null;
            }
            public void SetSpriteRenderer(SpriteRenderer leftFoot, SpriteRenderer rightFoot)
            {
                Dispose();
                spriteRenderers[0] = leftFoot;
                spriteRenderers[1] = rightFoot;
            }

            private void Dispose()
            {
                if (spriteRenderers[0]) Destroy(spriteRenderers[0].gameObject);
                if (spriteRenderers[1]) Destroy(spriteRenderers[1].gameObject);
            }
        }
    }
}
