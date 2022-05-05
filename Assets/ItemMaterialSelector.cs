using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class ItemMaterialSelector : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] Quads = new MeshRenderer[8];
        [SerializeField] private Transform currentQuad;
        private Vector3 currentQuadOffset = new Vector3(0, 0, 0.01f);//zファイ対策
        [SerializeField] private TextMesh textMesh;
        [SerializeField] private Animator anime;
        private int current = 0;
        private int limitTex;

        public int Current {
            get { return current; }
            set 
            {
                if (current == value) return;
                current = value;
                currentQuad.parent = Quads[current].transform;
                currentQuad.transform.localPosition = currentQuadOffset;
                currentQuad.transform.localRotation = Quaternion.identity; 
            }
        }
        public Texture GetCurrentTex()
        {
            if (current >= limitTex) return null;
            else return Quads[current].material.GetTexture("_BaseMap");
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        public void Init(DecorationItemInfo info)
        {
            current = 0;
            textMesh.text = info.itemName[(int)SystemInfo.userProfile.data.LanguageCode];

            limitTex = info.texs.Length;

            for (int i = 0; i < Quads.Length; i++)
            {
                if (i < limitTex)
                {
                    if(!Quads[i].gameObject.activeSelf)Quads[i].gameObject.SetActive(true);
                    Quads[i].material.SetTexture("_BaseMap", info.texs[i]);
                }
                else
                {
                    if (Quads[i].gameObject.activeSelf) Quads[i].gameObject.SetActive(false);
                } 
            }

            currentQuad.parent = Quads[current].transform;
            currentQuad.transform.localPosition = currentQuadOffset;
            currentQuad.transform.localRotation = Quaternion.identity;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}