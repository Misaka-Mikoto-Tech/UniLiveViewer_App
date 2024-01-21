using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NanaCiel;
using System;
using System.Linq;
using UnityEngine.Rendering;

namespace UniLiveViewer 
{
    //TODO:UI使いにくいので改善する、しばらくオミット
    public class PrefabEditor : MonoBehaviour
    {
        [SerializeField] private Transform vrmPresetAnchor;//マテリアル調整時用のキャラ座標アンカー
        [SerializeField] private RollSelector rollSelector;
        [SerializeField] private Button_Switch[] btn_SuefaceType = new Button_Switch[2];
        [SerializeField] private Button_Switch[] btn_RenderFace = new Button_Switch[3];
        [SerializeField] private Button_Switch[] btn_Cutoff = new Button_Switch[2];
        [SerializeField] private SliderGrabController slider_Transparent = null;
        [SerializeField] private SliderGrabController slider_Cutoff = null;
        [SerializeField] private Button_Base btn_AllReset;

        [SerializeField] private LineRenderer lineRenderer = null;
        [SerializeField] private Vector3 localOffset;
        [SerializeField] private float lineWeight_Second = 0.2f;
        [SerializeField] private float lineWeight_Third = 0.7f;
        private Vector3[] linePos = new Vector3[3];
        private Vector3 endpoint, dir, result;

        //public CharaController EditTarget => editTarget;
        //private CharaController editTarget;
        private MaterialManager matManager;

        private string currentMatName;

        public event Action onCurrentUpdate;

        private void Awake()
        {
            foreach (var e in btn_SuefaceType)
            {
                e.onTrigger += MaterialSetting_Change;
            }
            foreach (var e in btn_RenderFace)
            {
                e.onTrigger += MaterialSetting_Change;
            }
            foreach (var e in btn_Cutoff)
            {
                e.onTrigger += MaterialSetting_Change;
            }

            slider_Transparent.ValueUpdate += () =>
            {
                //透明を更新
                matManager.SetColor_Transparent(currentMatName, slider_Transparent.Value);
            };
            slider_Cutoff.ValueUpdate += () =>
            {
                //透明を更新
                matManager.SetCutoffVal(currentMatName, slider_Cutoff.Value);
            };
            rollSelector.onTouch += MaterialInfoUpdate;
            btn_AllReset.onTrigger += MaterialSetting_AllReset;

            lineRenderer.positionCount = 3;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            LineDraw();
        }

        private void LineDraw()
        {
            endpoint = vrmPresetAnchor.position + localOffset;
            dir = endpoint - lineRenderer.transform.position;

            linePos[0] = lineRenderer.transform.position;

            result = linePos[0].GetHorizontalDirection() + dir.GetHorizontalDirection() * lineWeight_Second;
            result.y = endpoint.y;
            linePos[1] = result;

            result = linePos[0].GetHorizontalDirection() + dir.GetHorizontalDirection() * lineWeight_Third;
            result.y = endpoint.y;
            linePos[2] = result;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, linePos[i]);
            }
        }

        //public void SetEditingTarget(CharaController _editTarget)
        //{
        //    //VRMをプリセットアンカーに移動
        //    //editTarget = _editTarget;
        //    ////editTarget.SetState(CharaEnums.STATE.NULL, vrmPresetAnchor);
        //    //matManager = editTarget.transform.GetComponent<MaterialManager>();

        //    //回転UI初期化
        //    List<string> list_mat = new List<string>(matManager.matLocation.Keys);
        //    rollSelector.Init(list_mat);

        //    MaterialInfoUpdate();
        //}

        /// <summary>
        /// マテリアルの表示情報を更新する
        /// </summary>
        private void MaterialInfoUpdate()
        {
            currentMatName = rollSelector.GetCurrentMatName();
            localOffset = matManager.matLocation[currentMatName] * FileReadAndWriteUtility.UserProfile.InitCharaSize;

            onCurrentUpdate?.Invoke();

            //恐らく全て同じという前提
            var matInfo = matManager.info.FirstOrDefault(x => x.name == currentMatName);
            //全取得
            int _index = matInfo.index;
            SurfaceType surfaceType = (SurfaceType)matInfo.skinMesh.materials[_index].GetFloat("_Surface");
            CullMode renderFace = (CullMode)matInfo.skinMesh.materials[_index].GetFloat("_Cull");
            Color color = matInfo.skinMesh.materials[_index].GetColor("_Color");
            bool alphaClip = matInfo.skinMesh.materials[_index].GetFloat("_AlphaClip") == 1 ? true : false;
            float cutoff = matInfo.skinMesh.materials[_index].GetFloat("_Cutoff");

            //UIに反映
            for (int i = 0; i < btn_SuefaceType.Length; i++)
            {
                if (i == (int)surfaceType) btn_SuefaceType[i].isEnable = true;
                else btn_SuefaceType[i].isEnable = false;
            }

            if (surfaceType == SurfaceType.Opaque) slider_Transparent.gameObject.SetActive(false);
            else if (surfaceType == SurfaceType.Transparent)
            {
                slider_Transparent.gameObject.SetActive(true);
                slider_Transparent.Value = color.a;
            }

            for (int i = 0; i < btn_RenderFace.Length; i++)
            {
                if (i == (int)renderFace) btn_RenderFace[i].isEnable = true;
                else btn_RenderFace[i].isEnable = false;
            }


            if (alphaClip)
            {
                btn_Cutoff[0].isEnable = true;
                btn_Cutoff[1].isEnable = false;
                slider_Cutoff.gameObject.SetActive(true);
                slider_Cutoff.Value = cutoff;
            }
            else
            {
                btn_Cutoff[0].isEnable = false;
                btn_Cutoff[1].isEnable = true;
                slider_Cutoff.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// マテリアル設定を変更
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_Change(Button_Base btn)
        {
            if (btn == btn_SuefaceType[0])
            {
                matManager.SetSurface(currentMatName, SurfaceType.Opaque);
                slider_Transparent.gameObject.SetActive(false);
                btn_SuefaceType[1].isEnable = false;
            }
            else if (btn == btn_SuefaceType[1])
            {
                matManager.SetSurface(currentMatName, SurfaceType.Transparent);
                slider_Transparent.gameObject.SetActive(true);
                btn_SuefaceType[0].isEnable = false;
            }

            if (btn == btn_RenderFace[0])
            {
                matManager.SetRenderFace(currentMatName, CullMode.Off);
                btn_RenderFace[1].isEnable = false;
                btn_RenderFace[2].isEnable = false;
            }
            else if (btn == btn_RenderFace[1])
            {
                matManager.SetRenderFace(currentMatName, CullMode.Front);
                btn_RenderFace[0].isEnable = false;
                btn_RenderFace[2].isEnable = false;
            }
            else if (btn == btn_RenderFace[2])
            {
                matManager.SetRenderFace(currentMatName, CullMode.Back);
                btn_RenderFace[0].isEnable = false;
                btn_RenderFace[1].isEnable = false;
            }

            if (btn == btn_Cutoff[0])
            {
                matManager.SetCutoff(currentMatName, 1);
                slider_Cutoff.gameObject.SetActive(true);
                btn_Cutoff[1].isEnable = false;
            }
            else if (btn == btn_Cutoff[1])
            {
                matManager.SetCutoff(currentMatName, 0);
                slider_Cutoff.gameObject.SetActive(false);
                btn_Cutoff[0].isEnable = false;
            }
        }

        /// <summary>
        /// マテリアル設定をリセット
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_AllReset(Button_Base btn)
        {
            ////クリック音
            //audioSource.PlayOneShot(Sound[0]);

            ////マテリアルをリセット
            //matConverter.ResetMaterials();

            ////UI表示を更新
            //MaterialInfoUpdate();
        }
    }
}
