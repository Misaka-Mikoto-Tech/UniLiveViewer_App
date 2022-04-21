using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer 
{
    public class PrefabEditor : MonoBehaviour
    {
        [SerializeField] private Transform vrmPresetAnchor;//マテリアル調整時用のキャラ座標アンカー
        [SerializeField] private RollSelector rollSelector_Material;
        [SerializeField] private Button_Switch[] btn_SuefaceType = new Button_Switch[2];
        [SerializeField] private Button_Switch[] btn_RenderFace = new Button_Switch[3];
        [SerializeField] private SliderGrabController slider_Transparent = null;
        [SerializeField] private Button_Base btn_AllReset;

        public CharaController EditTarget => editTarget;
        [SerializeField]private CharaController editTarget;

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
            slider_Transparent.ValueUpdate += MaterialSetting_TransparentColor;
            rollSelector_Material.onTouch += MaterialInfoUpdate;
            btn_AllReset.onTrigger += MaterialSetting_AllReset;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetEditingTarget(CharaController _editTarget)
        {
            //VRMをプリセットアンカーに移動
            editTarget = _editTarget;
            editTarget.SetState(CharaController.CHARASTATE.NULL, vrmPresetAnchor);
        }

        public void Init()
        {
            if (!editTarget) return;

            //回転UI初期化
            var matLocation = editTarget.GetComponent<MaterialManager>().matLocation;
            List<string> list = new List<string>(matLocation.Keys);
            rollSelector_Material.Init(list);
        }

        /// <summary>
        /// マテリアルの表示情報を更新する
        /// </summary>
        private void MaterialInfoUpdate()
        {
            ////クリック音
            //audioSource.PlayOneShot(Sound[0]);

            //int current = rollSelector_Material.current;
            //var type = (MaterialConverter.SurfaceType)matConverter.materials[current].GetFloat("_Surface");
            //var face = (MaterialConverter.RenderFace)matConverter.materials[current].GetFloat("_Cull");
            //var color = matConverter.materials[current].GetColor("_BaseColor");

            ////buttonに反映
            //if (type == MaterialConverter.SurfaceType.Opaque)
            //{
            //    btn_SuefaceType[0].isEnable = true;
            //    btn_SuefaceType[1].isEnable = false;
            //    //スライダー無効化
            //    slider_Transparent.gameObject.SetActive(false);
            //}
            //else if (type == MaterialConverter.SurfaceType.Transparent)
            //{
            //    btn_SuefaceType[0].isEnable = false;
            //    btn_SuefaceType[1].isEnable = true;
            //    //スライダー有効化
            //    slider_Transparent.gameObject.SetActive(true);
            //    slider_Transparent.Value = color.a;
            //}
            ////buttonに反映
            //if (face == MaterialConverter.RenderFace.Front)
            //{
            //    btn_RenderFace[0].isEnable = true;
            //    btn_RenderFace[1].isEnable = false;
            //    btn_RenderFace[2].isEnable = false;
            //}
            //else if (face == MaterialConverter.RenderFace.Back)
            //{
            //    btn_RenderFace[0].isEnable = false;
            //    btn_RenderFace[1].isEnable = true;
            //    btn_RenderFace[2].isEnable = false;
            //}
            //else if (face == MaterialConverter.RenderFace.Both)
            //{
            //    btn_RenderFace[0].isEnable = false;
            //    btn_RenderFace[1].isEnable = false;
            //    btn_RenderFace[2].isEnable = true;
            //}
        }

        /// <summary>
        /// マテリアル設定を変更
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_Change(Button_Base btn)
        {
            //int current = rollSelector_Material.current;

            //if (btn == btn_SuefaceType[0])
            //{
            //    matConverter.SetSurface(current, MaterialConverter.SurfaceType.Opaque);
            //}
            //else if (btn == btn_SuefaceType[1])
            //{
            //    matConverter.SetSurface(current, MaterialConverter.SurfaceType.Transparent);
            //}
            //else if (btn == btn_RenderFace[0])
            //{
            //    matConverter.SetRenderFace(current, MaterialConverter.RenderFace.Front);
            //}
            //else if (btn == btn_RenderFace[1])
            //{
            //    matConverter.SetRenderFace(current, MaterialConverter.RenderFace.Back);
            //}
            //else if (btn == btn_RenderFace[2])
            //{
            //    matConverter.SetRenderFace(current, MaterialConverter.RenderFace.Both);
            //}

            ////UI表示を更新
            //MaterialInfoUpdate();
        }

        /// <summary>
        /// マテリアルの透明色を設定
        /// </summary>
        private void MaterialSetting_TransparentColor()
        {
            ////透明を更新
            //matConverter.SetColor_Transparent(rollSelector_Material.current, slider_Transparent.Value);
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
