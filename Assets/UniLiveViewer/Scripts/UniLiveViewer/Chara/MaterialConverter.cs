using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }
    public enum BlendMode_MToon
    {
        Opaque,
        Cutout,
        Transparent,
        TransparentWithZWrite
    }
    public enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }
    public enum RenderFace//この並びはURP
    {
        Both,
        Back,
        Front
    }

    //ちゃんとURP用shader作ったら不要
    public class MaterialConverter : MonoBehaviour
    {

        public List<Material> materials { get; private set; } = new List<Material>();
        public List<Material> materials_Base { get; private set; } = new List<Material>();//リセット用に取得
        public static int layer_Default;

        [SerializeField]
        private Dictionary<BlendMode_MToon, BlendMode> replaceBlendmode = new Dictionary<BlendMode_MToon, BlendMode>()
        {
            {BlendMode_MToon.Opaque, BlendMode.Alpha},
            {BlendMode_MToon.Cutout, BlendMode.Premultiply},
            {BlendMode_MToon.Transparent, BlendMode.Additive},
            {BlendMode_MToon.TransparentWithZWrite, BlendMode.Multiply},
        };
        private Dictionary<string, Shader> fallbackShader = new Dictionary<string, Shader>();
        private Shader fallbackShader_default;

        bool alphaBlend;
        BlendMode blendMode;
        bool alphaClip ;
        float cutoffVal;
        RenderFace renderFace;
        string renderType;
        int zWrite;
        int _SrcBlend;
        int _DstBlend;
        bool _ALPHATEST_ON;
        bool _ALPHABLEND_ON;
        bool _ALPHAPREMULTIPLY_ON;
        bool ShadowCaster;
        int renderQueue;

        public void Init()
        {
            fallbackShader_default = Shader.Find("Shader Graphs/Simple Standard");

            fallbackShader.Add("VRM/MToon", Shader.Find("Shader Graphs/Simple MToon"));
            fallbackShader.Add("Standard", fallbackShader_default);
            fallbackShader.Add("Universal Render Pipeline/Unlit", fallbackShader_default);
            fallbackShader.Add("Universal Render Pipeline/Lit", fallbackShader_default);
            fallbackShader.Add("Universal Render Pipeline/Simple Lit", fallbackShader_default);

            layer_Default = SystemInfo.layerNo_Default;

            //replaceShader.Add(Shader.Find("Shader Graphs/Simple MToon"));
            //replaceShader.Add(Shader.Find("Shader Graphs/Simple Standard"));
            //replaceShader = Shader.Find("Shader Graphs/Simple Toon_DoubleShadow");
            //replaceShader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        public async UniTask Conversion(CharaController charaCon, CancellationToken token)
        {
            try
            {
                //前処理
                await Pretreatment(charaCon, token);
                //置換
                await ShaderReplace(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw new Exception("VRM matConversion");
            }
        }

        /// <summary>
        /// 前処理
        /// </summary>
        /// <param name="parent"></param>
        private async UniTask Pretreatment(CharaController charaCon, CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                foreach (var mesh in charaCon.GetSkinnedMeshRenderers)
                {
                    //あれば不要なので無効化しておく
                    
                    if (mesh.transform.childCount > 0 && mesh.transform.GetChild(0).transform.name.Contains("_headless"))
                    {
                        mesh.transform.GetChild(0).gameObject.SetActive(false);
                    }
                    //レイヤー設定
                    if (mesh.transform.name.Contains("eye", StringComparison.OrdinalIgnoreCase) 
                        || mesh.transform.name.Contains("face", StringComparison.OrdinalIgnoreCase))
                    {
                        //目や顔にアウトラインは残念な感じになりやすいので設定しない
                        mesh.gameObject.layer = layer_Default;
                    }
                    else
                    {
                        mesh.gameObject.layer = gameObject.layer;
                    }
                    //マテリアル取得
                    foreach (var mat in mesh.materials)
                    {
                        materials_Base.Add(mat);
                    }
                    foreach (var mat in mesh.materials)
                    {
                        materials.Add(mat);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private async UniTask ShaderReplace(CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //置換対象マテリアルを処理
                if (materials.Count > 0)
                {
                    for (int i = 0; i < materials.Count; i++)
                    {
                        SetupMaterialBlendMode(materials[i]);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async UniTask Conversion_Item(MeshRenderer[] meshRenderers, CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                foreach (var mesh in meshRenderers)
                {
                    mesh.gameObject.layer = gameObject.layer;
                    for (int i = 0; i < mesh.materials.Length; i++)
                    {
                        SetupMaterialBlendMode(mesh.materials[i]);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw new Exception("VRM matConversion");
            }
        }

        /// <summary>
        /// マテリアルの設定値を変更前に戻す
        /// </summary>
        public void ResetMaterials()
        {
            if (materials.Count != materials_Base.Count) return;

            for (int i = 0; i < materials.Count; i++)
            {
                //ベースから設定値をコピー
                materials[i].SetFloat("_Surface", (float)materials_Base[i].GetFloat("_Surface"));
                materials[i].SetFloat("_Blend", (float)materials_Base[i].GetFloat("_Blend"));
                materials[i].SetFloat("_Cull", (float)materials_Base[i].GetFloat("_Cull"));
                materials[i].color = materials_Base[i].color;

                //調整
                SetupMaterialBlendMode(materials[i]);
            }
        }

        /// <summary>
        /// サーフェスタイプを設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="type"></param>
        //public void SetSurface(int current, SurfaceType type)
        //{
        //    //SurfaceType
        //    materials[current].SetFloat("_Surface", (float)type);

        //    //調整
        //    SetupMaterialBlendMode(materials[current]);
        //}

        /// <summary>
        /// ブレンドモードを設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="mode"></param>
        //public void SetBlendMode(int current, BlendMode mode)
        //{
        //    //BlendMode
        //    materials[current].SetFloat("_Blend", (float)mode);

        //    //調整
        //    SetupMaterialBlendMode(materials[current]);
        //}

        /// <summary>
        /// 描画面を設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="render"></param>
        //public void SetRenderFace(int current, RenderFace render)
        //{
        //    //RenderFace
        //    materials[current].SetFloat("_Cull", (float)render);

        //    //調整
        //    //SetupMaterialBlendMode(materials[current]);
        //}

        /// <summary>
        /// カラーのアルファのみ設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="alpha"></param>
        //public void SetColor_Transparent(int current, float alpha)
        //{
        //    //透明調整
        //    Color col = materials[current].color;
        //    col.a = alpha;
        //    materials[current].color = col;
        //}

        /// <summary>
        /// 参考
        /// ttps://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html
        /// </summary>
        void SetupMaterialBlendMode(Material material)
        {
            var replaceShader = fallbackShader.FirstOrDefault(x => x.Key == material.shader.name).Value;
            if (!replaceShader) return;

            if (replaceShader == fallbackShader_default)
            {
                material.shader = replaceShader;
                return;
            }

            //Transparent
            alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
            BlendMode blendMode = replaceBlendmode[(BlendMode_MToon)material.GetFloat("_BlendMode")];
            //cut out
            alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
            cutoffVal = material.GetFloat("_Cutoff");

            renderFace = (RenderFace)material.GetFloat("_CullMode");

            renderType = material.GetTag("RenderType", true);

            zWrite = material.GetInt("_ZWrite");
            _SrcBlend = material.GetInt("_SrcBlend");
            _DstBlend = material.GetInt("_DstBlend");
            _ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
            _ALPHABLEND_ON = material.IsKeywordEnabled("_ALPHABLEND_ON");
            _ALPHAPREMULTIPLY_ON = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
            ShadowCaster = material.GetShaderPassEnabled("ShadowCaster");
            renderQueue = material.renderQueue;

            // ### debug用 ###
            //Debug.Log("-------------------------------------------------------------");
            //Debug.Log($"{material.name}→Transparent:{alphaBlend} / cut out:{alphaClip} / blendMode:{blendMode} / RenderType:{renderType} / renderFace:{renderFace}");
            //Debug.Log($"zWrite:{zWrite} / _Src:{_SrcBlend} / _Dst:{_DstBlend} / _ALPHATEST_ON:{_ALPHATEST_ON} / _ALPHABLEND_ON:{_ALPHABLEND_ON} / _ALPHAPREMULTIPLY_ON:{_ALPHAPREMULTIPLY_ON} / ShadowCaster:{ShadowCaster} / renderQueue:{renderQueue}");

            //影texture必須なので調整する
            if (material.GetTexture("_ShadeTexture") == null) material.SetTexture("_ShadeTexture", material.GetTexture("_MainTex"));

            //置換
            material.shader = replaceShader;

            //////////////////////
            material.SetOverrideTag("RenderType", renderType);
            if (alphaBlend) material.SetFloat("_Surface", (float)SurfaceType.Transparent);
            else material.SetFloat("_Surface", (float)SurfaceType.Opaque);

            material.SetFloat("_Cutoff", cutoffVal);
            //material.SetFloat("_Blend", (float)blendMode);
            material.SetFloat("_Blend", (float)BlendMode.Alpha);//他が上手くいかない
            material.SetFloat("_Cull", (float)renderFace);//URPで変わっている注意
            material.SetInt("_ZWrite", zWrite);
            material.SetInt("_SrcBlend", _SrcBlend);
            material.SetInt("_DstBlend", _DstBlend);

            if(_ALPHATEST_ON) material.EnableKeyword("_ALPHATEST_ON");
            else material.DisableKeyword("_ALPHATEST_ON");
            if (_ALPHABLEND_ON) material.EnableKeyword("_ALPHABLEND_ON");
            else material.DisableKeyword("_ALPHABLEND_ON");
            if (_ALPHAPREMULTIPLY_ON) material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            else material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if (ShadowCaster) material.EnableKeyword("ShadowCaster");
            else material.DisableKeyword("ShadowCaster");

            material.renderQueue = renderQueue;

            if(alphaClip) material.SetFloat("_AlphaClip", 1);
            else material.SetFloat("_AlphaClip", 0);


            // ### debug用 ###
            //Transparent
            //alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
            //blendMode = (BlendMode)material.GetFloat("_Blend");
            ////cut out
            //alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
            //cutoffVal = material.GetFloat("_Cutoff");

            //renderFace = (RenderFace)material.GetFloat("_Cull");

            //renderType = material.GetTag("RenderType", true);

            //zWrite = material.GetInt("_ZWrite");
            //_SrcBlend = material.GetInt("_SrcBlend");
            //_DstBlend = material.GetInt("_DstBlend");
            //_ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
            //_ALPHABLEND_ON = material.IsKeywordEnabled("_ALPHABLEND_ON");
            //_ALPHAPREMULTIPLY_ON = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
            //ShadowCaster = material.GetShaderPassEnabled("ShadowCaster");
            //renderQueue = material.renderQueue;

            //Debug.Log($"{material.name}→Transparent:{alphaBlend} / cut out:{alphaClip} / blendMode:{blendMode} / RenderType:{renderType} / renderFace:{renderFace}");
            //Debug.Log($"zWrite:{zWrite} / _Src:{_SrcBlend} / _Dst:{_DstBlend} / _ALPHATEST_ON:{_ALPHATEST_ON} / _ALPHABLEND_ON:{_ALPHABLEND_ON} / _ALPHAPREMULTIPLY_ON:{_ALPHAPREMULTIPLY_ON} / ShadowCaster:{ShadowCaster} / renderQueue:{renderQueue}");
            //Debug.Log("-------------------------------------------------------------");
        }

        /// <summary>
        /// 参考
        /// ttps://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html
        /// </summary>
        /// <param name="material"></param>
        void SetupMaterialBlendMode_old(Material material)
        {
            //Transparent
            bool alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
            BlendMode blendMode =  (BlendMode)material.GetFloat("_BlendMode");
            //cut out
            bool alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
            float cutoffVal = material.GetFloat("_Cutoff");

            //置換
            var replaceShader = fallbackShader.FirstOrDefault(x => x.Key == material.shader.name).Value;
            if (!replaceShader) replaceShader = fallbackShader_default;
            material.shader = replaceShader;

            //調整
            if (alphaClip)
            {
                //material.SetFloat("_Surface", (float)SurfaceType.Opaque);
                material.EnableKeyword("_ALPHATEST_ON");
                material.SetFloat("_AlphaClip", 1);
                material.SetFloat("_Cutoff", cutoffVal);
            }
            else if (alphaBlend)
            {
                material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                //material.EnableKeyword("_ALPHAPREMULTIPLY_ON");//こっちっぽいのに下のが正常な結果
                material.EnableKeyword("_ALPHATEST_ON");
            }
            else
            {
                //material.SetFloat("_Surface", (float)SurfaceType.Opaque);
            }

            Debug.Log($"{material.name}の詳細→Transparent:{alphaBlend} / cut out:{alphaClip} / z:{blendMode}");
            //SurfaceType surfaceType = (SurfaceType)material.SetFloat("_Surface",0);

            if (!alphaBlend)
            {
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                material.SetShaderPassEnabled("ShadowCaster", true);
            }
            else
            {
                //BlendMode blendMode = (BlendMode)material.GetFloat("_Blend"); //URP

                switch (blendMode)
                {
                    case BlendMode.Alpha:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Premultiply:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Additive:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                    case BlendMode.Multiply:
                        material.SetOverrideTag("RenderType", "Transparent");
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        material.SetShaderPassEnabled("ShadowCaster", false);
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            //for (int i = 0; i < materials.Count;i++)
            //{
            //    Destroy(materials[i]);
            //}
            //materials.Clear();

            //for (int i = 0; i < materials_Base.Count; i++)
            //{
            //    Destroy(materials_Base[i]);
            //}
            //materials_Base.Clear();
        }
    }
}