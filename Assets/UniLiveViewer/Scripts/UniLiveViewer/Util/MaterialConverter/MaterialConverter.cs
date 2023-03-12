using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using NanaCiel;
using UnityEngine.Rendering;

namespace UniLiveViewer
{
    public class MaterialConverter : IMaterialConverter
    {
        /// <summary>
        /// 現在のマテリアル
        /// </summary>
        List<Material> _materials;
        /// <summary>
        /// 初期化用のマテリアル（いる？）
        /// </summary>
        List<Material> _materials_Base; 

        readonly Dictionary<BlendMode_MToon, BlendMode> _replaceBlendMode;
        readonly Dictionary<string, Shader> _convertMap;

        int _myLayer;

        bool _ALPHATEST_ON;
        bool _ALPHABLEND_ON;
        bool _ALPHAPREMULTIPLY_ON;
        bool _alphaBlend;
        bool _alphaClip;

        /// <summary>
        /// 使わないかも
        /// </summary>
        BlendMode _blendMode;
        CullMode _renderFace;
        /// <summary>
        /// 使わないかも
        /// </summary>
        string _renderType;
        
        int _zWrite;
        int _SrcBlend;
        int _DstBlend;

        Texture _mainTex;
        Color _mainColor;

        Texture _shadeTexture;
        Color _shadeColor;

        Texture _emissionTex;
        Color _emissionColor;

        
        bool _shadowCaster;
        /// <summary>
        /// 使わないかも
        /// </summary>
        int _renderQueue;
        float _cutOff;

        Vector2 _tiling;
        Vector2 _offset;
        float _uvAnimScrollX;
        float _uvAnimScrollY;

        public MaterialConverter(int myLayer)
        {
            _myLayer = myLayer;

            _materials = new List<Material>();
            _materials_Base = new List<Material>();//リセット用に取得

            _replaceBlendMode = new Dictionary<BlendMode_MToon, BlendMode>()
            {
                {BlendMode_MToon.Opaque, BlendMode.Alpha},
                {BlendMode_MToon.Cutout, BlendMode.Premultiply},
                {BlendMode_MToon.Transparent, BlendMode.Additive},
                {BlendMode_MToon.TransparentWithZWrite, BlendMode.Multiply},
            };

            var defaultFallbackShader = Shader.Find("Shader Graphs/Simple Standard");
            _convertMap = new Dictionary<string, Shader>()
            {
                { "VRM/MToon", Shader.Find("Shader Graphs/Simple MToon" )},
                { "Standard", defaultFallbackShader},
                { "Universal Render Pipeline/Unlit", defaultFallbackShader},
                { "Universal Render Pipeline/Lit", defaultFallbackShader},
                { "Universal Render Pipeline/Simple Lit", defaultFallbackShader}
                //Shader Graphs/Simple Standard
                //Shader Graphs/Simple Toon_DoubleShadow
            };
        }
        
        async UniTask IMaterialConverter.Conversion(CharaController charaCon, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await Pretreatment(charaCon, token).OnError();
            token.ThrowIfCancellationRequested();

            await ShaderReplace(token).OnError();
        }

        /// <summary>
        /// 前処理
        /// </summary>
        /// <param name="charaCon"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask Pretreatment(CharaController charaCon, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            foreach (var mesh in charaCon.GetSkinnedMeshRenderers)
            {
                //レイヤー設定
                if (mesh.transform.name.Contains("eye", StringComparison.OrdinalIgnoreCase)
                    || mesh.transform.name.Contains("face", StringComparison.OrdinalIgnoreCase))
                {
                    //目や顔にアウトラインは残念な感じになりやすいので
                    mesh.gameObject.layer = SystemInfo.layerNo_UnRendererFeature;
                }
                else mesh.gameObject.layer = _myLayer;

                //マテリアル取得
                foreach (var mat in mesh.materials)
                {
                    _materials_Base.Add(mat);
                    _materials.Add(mat);
                }
            }
        }

        /// <summary>
        /// 置換処理
        /// </summary>
        /// <param name="charaCon"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask ShaderReplace(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            for (int i = 0; i < _materials.Count; i++)
            {
                aaa(_materials[i]);
            }
        }

        async UniTask IMaterialConverter.Conversion_Item(MeshRenderer[] meshRenderers, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            token.ThrowIfCancellationRequested();

            foreach (var mesh in meshRenderers)
            {
                mesh.gameObject.layer = _myLayer;
                for (int i = 0; i < mesh.materials.Length; i++)
                {
                    aaa(mesh.materials[i]);
                }
            }
        }

        void aaa(Material material)
        {
            //変換後Shader情報
            var replaceShader = _convertMap.FirstOrDefault(x => x.Key == material.shader.name).Value;
            if (replaceShader is null) return;

            ReadPropertys(material);

            //置換
            material.shader = replaceShader;

            Setup(material);
        }

        /// <summary>
        /// 置換前のマテリアルから読み取り
        /// </summary>
        /// <param name="material"></param>
        void ReadPropertys(Material material)
        {
            //定義済みローカルキーワード
            _ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
            _ALPHABLEND_ON = material.IsKeywordEnabled("_ALPHABLEND_ON");
            _ALPHAPREMULTIPLY_ON = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");

            _zWrite = material.GetInt("_ZWrite");
            _SrcBlend = material.GetInt("_SrcBlend");
            _DstBlend = material.GetInt("_DstBlend");

            _shadowCaster = material.GetShaderPassEnabled("ShadowCaster");
            _renderQueue = material.renderQueue;

            //Rendering
            _blendMode = (BlendMode)material.GetFloat("_BlendMode");
            _renderFace = (CullMode)material.GetFloat("_CullMode");
            _renderType = material.GetTag("RenderType", true);

            //Color
            _mainTex = material.GetTexture("_MainTex");
            _mainColor = material.GetColor("_Color");
            _shadeTexture = material.GetTexture("_ShadeTexture");
            _shadeColor = material.GetColor("_ShadeColor");

            //Emission
            _emissionTex = material.GetTexture("_EmissionMap");
            _emissionColor = material.GetColor("_EmissionColor");

            //Transparent
            _alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
            _blendMode = _replaceBlendMode[(BlendMode_MToon)material.GetFloat("_BlendMode")];
            //cut out
            _alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
            _cutOff = material.GetFloat("_Cutoff");

            //UV Coordinates
            _tiling = material.GetTextureScale("_MainTex");
            _offset = material.GetTextureOffset("_MainTex");
            _uvAnimScrollX = material.GetFloat("_UvAnimScrollX");
            _uvAnimScrollY = material.GetFloat("_UvAnimScrollY");
        }

        /// <summary>
        /// 置換後マテリアルに設定
        /// </summary>
        /// <param name="material"></param>
        void Setup(Material material)
        {
            material.SetTexture("_MainTex", _mainTex);
            material.SetColor("_Color", _mainColor);

            //影texture必須なので調整する
            if (_shadeTexture is null) _shadeTexture = _mainTex;
            material.SetTexture("_ShadeTexture", _shadeTexture);
            material.SetColor("_ShadeColor", _shadeColor);

            if(_emissionTex is Texture && _shadeColor != Color.black)
            {
                material.SetTexture("_EmissionMap", _emissionTex);
                material.SetColor("_EmissionColor", _emissionColor);
            }

            material.SetVector("_tiling", _tiling);
            material.SetVector("_offset", _offset);
            material.SetFloat("_UvAnimScrollX", _uvAnimScrollX);
            material.SetFloat("_UvAnimScrollY", _uvAnimScrollY);

            material.SetFloat("_Cull", (float)_renderFace);
            if (!_alphaBlend)
            {
                material.SetOverrideTag("RenderType", "Opaque");

                //Opaque
                if (!_alphaClip)
                {
                    material.SetFloat("_Surface", (float)SurfaceType.Opaque);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_AlphaClip", 0);

                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.renderQueue = -1;
                }
                //Cutoff
                else
                {
                    material.SetFloat("_Surface", (float)SurfaceType.Opaque);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_AlphaClip", 1);
                    material.SetFloat("_Cutoff", _cutOff);

                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.renderQueue = (int)RenderQueue.AlphaTest;
                }
            }
            else if (_alphaBlend)
            {
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetFloat("_Blend", (float)BlendMode.Alpha);//透明なら必要、他だと上手くいかない
                material.SetFloat("_AlphaClip", 0);

                //Transparent
                if (_zWrite == 0)
                {
                    //material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                    //material.SetInt("_ZWrite", 0);
                    //material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    //material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                    //material.EnableKeyword("_ALPHABLEND_ON");
                    //material.DisableKeyword("_ALPHATEST_ON");
                    //material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    //material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                    material.SetOverrideTag("RenderType", "");

                    material.SetFloat("_Surface", (float)SurfaceType.Opaque);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_AlphaClip", 1);
                    material.SetFloat("_Cutoff", _cutOff);

                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.renderQueue = (int)RenderQueue.AlphaTest;
                }
                //TransparentWithZWrite
                else if (_zWrite == 1)
                {
                    //material.SetFloat("_Surface", (float)SurfaceType.Transparent);
                    //material.SetInt("_ZWrite", 1);
                    //material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    //material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

                    //material.EnableKeyword("_ALPHABLEND_ON");
                    //material.DisableKeyword("_ALPHATEST_ON");
                    //material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    //material.renderQueue = 2501;

                    material.SetOverrideTag("RenderType", "");

                    material.SetFloat("_Surface", (float)SurfaceType.Opaque);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_AlphaClip", 1);
                    material.SetFloat("_Cutoff", _cutOff);

                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                    material.renderQueue = (int)RenderQueue.AlphaTest;

                }
            }
        }


        //private void debug(Material material)
        //{
        //    _alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
        //    _blendMode = (BlendMode)material.GetFloat("_Blend");
        //    _alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
        //    _cutOff = material.GetFloat("_Cutoff");

        //    _renderFace = (CullMode)material.GetFloat("_Cull");

        //    _renderType = material.GetTag("RenderType", true);

        //    _zWrite = material.GetInt("_ZWrite");
        //    _SrcBlend = material.GetInt("_SrcBlend");
        //    _DstBlend = material.GetInt("_DstBlend");
        //    _ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
        //    _ALPHABLEND_ON = material.IsKeywordEnabled("_ALPHABLEND_ON");
        //    _ALPHAPREMULTIPLY_ON = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
        //    _shadowCaster = material.GetShaderPassEnabled("ShadowCaster");
        //    _renderQueue = material.renderQueue;

        //    Debug.Log($"{material.name}→Transparent:{_alphaBlend} / cut out:{_alphaClip} / blendMode:{_blendMode} / RenderType:{_renderType} / renderFace:{_renderFace}");
        //    Debug.Log($"zWrite:{_zWrite} / _Src:{_SrcBlend} / _Dst:{_DstBlend} / _ALPHATEST_ON:{_ALPHATEST_ON} / _ALPHABLEND_ON:{_ALPHABLEND_ON} / _ALPHAPREMULTIPLY_ON:{_ALPHAPREMULTIPLY_ON} / ShadowCaster:{_shadowCaster} / renderQueue:{_renderQueue}");
        //    Debug.Log("-------------------------------------------------------------");
        //}

        /// <summary>
        /// 参考
        /// ttps://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html
        /// </summary>
        //void SetupMaterialBlendMode(Material material)
        //{
        //    var replaceShader = _convertMap.FirstOrDefault(x => x.Key == material.shader.name).Value;
        //    if (!replaceShader) return;

        //    if (replaceShader == _defaultFallbackShader)
        //    {
        //        material.shader = replaceShader;
        //        return;
        //    }

        //    //Transparent
        //    _alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
        //    _blendMode = _replaceBlendMode[(BlendMode_MToon)material.GetFloat("_BlendMode")];
        //    //cut out
        //    _alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
        //    _cutOff = material.GetFloat("_Cutoff");

        //    _renderFace = (CullMode)material.GetFloat("_CullMode");

        //    _renderType = material.GetTag("RenderType", true);

        //    _zWrite = material.GetInt("_ZWrite");
        //    _SrcBlend = material.GetInt("_SrcBlend");
        //    _DstBlend = material.GetInt("_DstBlend");
        //    _ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
        //    _ALPHABLEND_ON = material.IsKeywordEnabled("_ALPHABLEND_ON");
        //    _ALPHAPREMULTIPLY_ON = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
        //    _shadowCaster = material.GetShaderPassEnabled("ShadowCaster");
        //    _renderQueue = material.renderQueue;

        //    // ### debug用 ###
        //    //Debug.Log("-------------------------------------------------------------");
        //    //Debug.Log($"{material.name}→Transparent:{alphaBlend} / cut out:{alphaClip} / blendMode:{blendMode} / RenderType:{renderType} / renderFace:{renderFace}");
        //    //Debug.Log($"zWrite:{zWrite} / _Src:{_SrcBlend} / _Dst:{_DstBlend} / _ALPHATEST_ON:{_ALPHATEST_ON} / _ALPHABLEND_ON:{_ALPHABLEND_ON} / _ALPHAPREMULTIPLY_ON:{_ALPHAPREMULTIPLY_ON} / ShadowCaster:{ShadowCaster} / renderQueue:{renderQueue}");

        //    //影texture必須なので調整する
        //    if (material.GetTexture("_ShadeTexture") == null) material.SetTexture("_ShadeTexture", material.GetTexture("_MainTex"));

        //    //置換
        //    material.shader = replaceShader;

        //    //////////////////////
        //    material.SetOverrideTag("RenderType", _renderType);
        //    if (_alphaBlend) material.SetFloat("_Surface", (float)SurfaceType.Transparent);
        //    else material.SetFloat("_Surface", (float)SurfaceType.Opaque);

        //    material.SetFloat("_Cutoff", _cutOff);
        //    //material.SetFloat("_Blend", (float)blendMode);
        //    material.SetFloat("_Blend", (float)BlendMode.Alpha);//他が上手くいかない
        //    //material.SetFloat("_Cull", (float)renderFace);//URPで変わっている注意
        //    material.SetInt("_ZWrite", _zWrite);
        //    material.SetInt("_SrcBlend", _SrcBlend);
        //    material.SetInt("_DstBlend", _DstBlend);

        //    if (false)
        //    {
        //        if (_ALPHATEST_ON) material.EnableKeyword("_ALPHATEST_ON");
        //        else material.DisableKeyword("_ALPHATEST_ON");
        //        if (_ALPHABLEND_ON) material.EnableKeyword("_ALPHABLEND_ON");
        //        else material.DisableKeyword("_ALPHABLEND_ON");
        //        if (_ALPHAPREMULTIPLY_ON) material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        //        else material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //        if (_shadowCaster) material.EnableKeyword("ShadowCaster");
        //        else material.DisableKeyword("ShadowCaster");

        //        material.renderQueue = _renderQueue;

        //        if (_alphaClip) material.SetFloat("_AlphaClip", 1);
        //        else material.SetFloat("_AlphaClip", 0);
        //    }


        //    // ### debug用 ###
        //    //Transparent
        //    _alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
        //    _blendMode = (BlendMode)material.GetFloat("_Blend");
        //    //cut out
        //    _alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
        //    _cutOff = material.GetFloat("_Cutoff");

        //    _renderFace = (CullMode)material.GetFloat("_Cull");

        //    _renderType = material.GetTag("RenderType", true);

        //    _zWrite = material.GetInt("_ZWrite");
        //    _SrcBlend = material.GetInt("_SrcBlend");
        //    _DstBlend = material.GetInt("_DstBlend");
        //    _ALPHATEST_ON = material.IsKeywordEnabled("_ALPHATEST_ON");
        //    _ALPHABLEND_ON = material.IsKeywordEnabled("_ALPHABLEND_ON");
        //    _ALPHAPREMULTIPLY_ON = material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
        //    _shadowCaster = material.GetShaderPassEnabled("ShadowCaster");
        //    _renderQueue = material.renderQueue;

        //    Debug.Log($"{material.name}→Transparent:{_alphaBlend} / cut out:{_alphaClip} / blendMode:{_blendMode} / RenderType:{_renderType} / renderFace:{_renderFace}");
        //    Debug.Log($"zWrite:{_zWrite} / _Src:{_SrcBlend} / _Dst:{_DstBlend} / _ALPHATEST_ON:{_ALPHATEST_ON} / _ALPHABLEND_ON:{_ALPHABLEND_ON} / _ALPHAPREMULTIPLY_ON:{_ALPHAPREMULTIPLY_ON} / ShadowCaster:{_shadowCaster} / renderQueue:{_renderQueue}");
        //    Debug.Log("-------------------------------------------------------------");
        //}

        ///// <summary>
        ///// 参考
        ///// ttps://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html
        ///// </summary>
        ///// <param name="material"></param>
        //void SetupMaterialBlendMode_old(Material material)
        //{
        //    //Transparent
        //    bool alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
        //    BlendMode blendMode =  (BlendMode)material.GetFloat("_BlendMode");
        //    //cut out
        //    bool alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;
        //    float cutoffVal = material.GetFloat("_Cutoff");

        //    //置換
        //    var replaceShader = _convertMap.FirstOrDefault(x => x.Key == material.shader.name).Value;
        //    if (!replaceShader) replaceShader = _defaultFallbackShader;
        //    material.shader = replaceShader;

        //    //調整
        //    if (alphaClip)
        //    {
        //        //material.SetFloat("_Surface", (float)SurfaceType.Opaque);
        //        material.EnableKeyword("_ALPHATEST_ON");
        //        material.SetFloat("_AlphaClip", 1);
        //        material.SetFloat("_Cutoff", cutoffVal);
        //    }
        //    else if (alphaBlend)
        //    {
        //        material.SetFloat("_Surface", (float)SurfaceType.Transparent);
        //        //material.EnableKeyword("_ALPHAPREMULTIPLY_ON");//こっちっぽいのに下のが正常な結果
        //        material.EnableKeyword("_ALPHATEST_ON");
        //    }
        //    else
        //    {
        //        //material.SetFloat("_Surface", (float)SurfaceType.Opaque);
        //    }

        //    Debug.Log($"{material.name}の詳細→Transparent:{alphaBlend} / cut out:{alphaClip} / z:{blendMode}");
        //    //SurfaceType surfaceType = (SurfaceType)material.SetFloat("_Surface",0);

        //    if (!alphaBlend)
        //    {
        //        material.SetOverrideTag("RenderType", "");
        //        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        //        material.SetInt("_ZWrite", 1);
        //        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //        material.renderQueue = -1;
        //        material.SetShaderPassEnabled("ShadowCaster", true);
        //    }
        //    else
        //    {
        //        //BlendMode blendMode = (BlendMode)material.GetFloat("_Blend"); //URP

        //        switch (blendMode)
        //        {
        //            case BlendMode.Alpha:
        //                material.SetOverrideTag("RenderType", "Transparent");
        //                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        //                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //                material.SetInt("_ZWrite", 0);
        //                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        //                material.SetShaderPassEnabled("ShadowCaster", false);
        //                break;
        //            case BlendMode.Premultiply:
        //                material.SetOverrideTag("RenderType", "Transparent");
        //                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        //                material.SetInt("_ZWrite", 0);
        //                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        //                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        //                material.SetShaderPassEnabled("ShadowCaster", false);
        //                break;
        //            case BlendMode.Additive:
        //                material.SetOverrideTag("RenderType", "Transparent");
        //                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        //                material.SetInt("_ZWrite", 0);
        //                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        //                material.SetShaderPassEnabled("ShadowCaster", false);
        //                break;
        //            case BlendMode.Multiply:
        //                material.SetOverrideTag("RenderType", "Transparent");
        //                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
        //                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        //                material.SetInt("_ZWrite", 0);
        //                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        //                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        //                material.SetShaderPassEnabled("ShadowCaster", false);
        //                break;
        //        }
        //    }
        //}

        /// <summary>
        /// マテリアルの設定値を変更前に戻す
        /// </summary>
        public void ResetMaterials()
        {
            //if (materials.Count != materials_Base.Count) return;

            //for (int i = 0; i < materials.Count; i++)
            //{
            //    //ベースから設定値をコピー
            //    materials[i].SetFloat("_Surface", (float)materials_Base[i].GetFloat("_Surface"));
            //    materials[i].SetFloat("_Blend", (float)materials_Base[i].GetFloat("_Blend"));
            //    materials[i].SetFloat("_Cull", (float)materials_Base[i].GetFloat("_Cull"));
            //    materials[i].color = materials_Base[i].color;

            //    //調整
            //    SetupMaterialBlendMode(materials[i]);
            //}
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

        void IMaterialConverter.Dispose()
        {
            
        }
    }
}