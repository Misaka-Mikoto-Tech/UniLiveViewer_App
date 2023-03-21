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
        enum ReplaceShaderType
        {
            None = -1,
            Default = 0,
            SimpleMToon,
        }

        /// <summary>
        /// 現在のマテリアル
        /// </summary>
        List<Material> _materials;
        /// <summary>
        /// 初期化用のマテリアル（いる？）
        /// </summary>
        List<Material> _materials_Base; 

        readonly Dictionary<string, ReplaceShaderType> _shaderMap;
        int _myLayer;

        // NOTE: 不要かな
        //bool _alphaTest;
        //bool _alphaBlend;

        BlendMode_MToon _blendMode;
        CullMode _renderFace;

        // NOTE: 不要かな
        //string _renderType;
        //int _zWrite;
        //int _SrcBlend;
        //int _DstBlend;
        //bool _shadowCaster;
        //int _renderQueue;

        //Color
        Texture _mainTex;
        Color _mainColor;
        Texture _shadeTexture;
        Color _shadeColor;
        float _cutOff;
        //Lighting
        float _shadeToony;
        float _shadeShift;
        //Emission
        Texture _emissionTex;
        Color _emissionColor;
        //Rim
        Color _rimColor;
        //UV Coordinates
        Vector2 _tiling;
        Vector2 _offset;
        //Auto Animation
        float _uvAnimScrollX;
        float _uvAnimScrollY;

        public MaterialConverter(int myLayer)
        {
            _myLayer = myLayer;

            _materials = new List<Material>();
            _materials_Base = new List<Material>();//リセット用に取得

            _shaderMap = new Dictionary<string, ReplaceShaderType>()
            {
                { "VRM/MToon", ReplaceShaderType.SimpleMToon},
                { "Standard", ReplaceShaderType.Default},
                { "Universal Render Pipeline/Unlit", ReplaceShaderType.Default},
                { "Universal Render Pipeline/Lit", ReplaceShaderType.Default},
                { "Universal Render Pipeline/Simple Lit", ReplaceShaderType.Default}
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
            for (int i = 0; i < _materials.Count; i++)
            {
                InternalConversion(_materials[i]);
            }
        }

        async UniTask IMaterialConverter.Conversion_Item(MeshRenderer[] meshRenderers, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
            foreach (var mesh in meshRenderers)
            {
                mesh.gameObject.layer = _myLayer;
                for (int i = 0; i < mesh.materials.Length; i++)
                {
                    InternalConversion(mesh.materials[i]);
                }
            }
        }

        void InternalConversion(Material material)
        {
            var replaceShaderType = _shaderMap.FirstOrDefault(x => x.Key == material.shader.name).Value;
            if (replaceShaderType is ReplaceShaderType.None)
            {
                Debug.LogWarning($"未対応Shaderです:{material.shader.name}");
                return;
            }

            ReadProperty(material);

            if (replaceShaderType == ReplaceShaderType.Default)
            {
                material.shader = Shader.Find("Shader Graphs/Simple Standard");
                SetPropertyToSimpleStandard(material);
            }
            else if (replaceShaderType == ReplaceShaderType.SimpleMToon)
            {
                // NOTE: デフォをtransparentにしておかないと効かない..？
                material.shader = Shader.Find("Shader Graphs/Simple MToon");
                SetPropertyToSimpleMToon(material);
            }
        }

        /// <summary>
        /// マテリアルからShader読み取り
        /// 
        /// TODO: MToon以外も対応する、Material.HasPropertyは...さぼる
        /// </summary>
        /// <param name="material"></param>
        void ReadProperty(Material material)
        {
            //定義済みローカルキーワード
            //_alphaTest = material.IsKeywordEnabled("_ALPHATEST_ON");
            //_alphaBlend = material.IsKeywordEnabled("_ALPHABLEND_ON");

            // NOTE: 多分不要
            //_zWrite = material.GetInt("_ZWrite");
            //_SrcBlend = material.GetInt("_SrcBlend");
            //_DstBlend = material.GetInt("_DstBlend");
            //_shadowCaster = material.GetShaderPassEnabled("ShadowCaster");
            //_renderQueue = material.renderQueue;

            //Rendering
            //_renderType = material.GetTag("RenderType", true);
            _blendMode = (BlendMode_MToon)material.GetFloat("_BlendMode");
            _renderFace = (CullMode)material.GetFloat("_CullMode");

            //Color
            _mainTex = material.GetTexture("_MainTex");
            _mainColor = material.GetColor("_Color");
            _shadeTexture = material.GetTexture("_ShadeTexture");
            _shadeColor = material.GetColor("_ShadeColor");
            _cutOff = material.GetFloat("_Cutoff");

            //Lighting
            _shadeToony = material.GetFloat("_ShadeToony");
            //var bumpMap = material.GetTexture("_BumpMap");
            //var bumpScale = material.GetFloat("_BumpScale");
            _shadeShift = material.GetFloat("_ShadeShift");
            //var receiveShadowTexture = material.GetTexture("_ReceiveShadowTexture");
            //var receiveShadowRate = material.GetFloat("_ReceiveShadowRate");
            //var shadingGradeTexture = material.GetTexture("_ShadingGradeTexture");
            //var shadingGradeRate = material.GetFloat("_ShadingGradeRate");
            //var lightColorAttenuation = material.GetFloat("_LightColorAttenuation");
            //var indirectLightIntensity = material.GetFloat("_IndirectLightIntensity");

            //Emission
            //var emissionTex = material.GetTexture("_EmissionMap");
            _emissionColor = material.GetColor("_EmissionColor");
            //var sphereAdd = material.GetTexture("_SphereAdd");

            //Rim
            //var rimTexture = material.GetTexture("_RimTexture");
            _rimColor = material.GetColor("_RimColor");
            //var rimLightingMix = material.GetFloat("_RimLightingMix");
            //var rimFresnelPower = material.GetFloat("_RimFresnelPower");
            //var rimLift = material.GetFloat("_RimLift");

            //Outline: あまり使いたくないなぁ
            //var outlineWidthMode = material.GetFloat("_OutlineWidthMode");
            //var outlineWidthTexture = material.GetTexture("_OutlineWidthTexture");
            //var outlineWidth = material.GetFloat("_OutlineWidth");
            //var outlineScaledMaxDistance = material.GetFloat("_OutlineScaledMaxDistance");
            //var outlineColorMode = material.GetFloat("_OutlineColorMode");
            //var outlineColor = material.GetColor("_OutlineColor");
            //var outlineLightingMix = material.GetFloat("_OutlineLightingMix");

            //Transparent
            //_alphaBlend = Array.IndexOf(material.shaderKeywords, "_ALPHABLEND_ON") != -1;
            //_blendMode = _blendModeMap[(BlendMode_MToon)material.GetFloat("_BlendMode")];
            //cut out
            //_alphaClip = Array.IndexOf(material.shaderKeywords, "_ALPHATEST_ON") != -1;

            //UV Coordinates
            _tiling = material.GetTextureScale("_MainTex");
            _offset = material.GetTextureOffset("_MainTex");

            //Auto Animation
            _uvAnimScrollX = material.GetFloat("_UvAnimScrollX");
            _uvAnimScrollY = material.GetFloat("_UvAnimScrollY");
            //var uvAnimRotation = material.GetFloat("_UvAnimRotation");

            //Options
            //var debugMode = material.GetFloat("_DebugMode");
            //var doubleSidedGI = material.doubleSidedGI;
            //var renderQueue = material.renderQueue;
        }

        void SetPropertyToSimpleStandard(Material material)
        {
            // TODO: パーツで混じってるので対応する

            //SetKeyword(material);
        }

        void SetPropertyToSimpleMToon(Material material)
        {
            material.SetTexture(URPShaderConstant.MAIN_TEX, _mainTex);
            material.SetColor(URPShaderConstant.COLOR, _mainColor);

            //影texture必須なので調整する
            if (_shadeTexture is null) _shadeTexture = _mainTex;
            material.SetTexture(URPShaderConstant.SHADE_TEX, _shadeTexture);
            material.SetColor(URPShaderConstant.SHADE_COLOR, _shadeColor);

            if (_emissionTex is Texture && _shadeColor != Color.black)
            {
                //material.SetTexture("_EmissionMap", _emissionTex); 負荷チェック
                material.SetColor(URPShaderConstant.EMISSION_COLOR, _emissionColor);
            }

            material.SetVector(URPShaderConstant.TILING, _tiling);
            material.SetVector(URPShaderConstant.OFFSET, _offset);
            material.SetFloat(URPShaderConstant.UV_ANIM_SCR_X, _uvAnimScrollX);
            material.SetFloat(URPShaderConstant.UV_ANIM_SCR_Y, _uvAnimScrollY);

            SetKeyword(material);
        }

        void SetKeyword(Material material)
        {
            if (_blendMode == BlendMode_MToon.Opaque)
            {
                material.SetOverrideTag(URPShaderConstant.RENDER_TYPE, "Opaque");

                material.SetFloat(URPShaderConstant.SURFACE, (float)SurfaceType.Opaque);
                //material.SetFloat(URPShaderConstant.BLEND_MODE, (float)BlendMode.Alpha);
                material.SetFloat(URPShaderConstant.ALPHA_CLIP, 0);
                material.SetInt(URPShaderConstant.SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt(URPShaderConstant.DST_BLEND, (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt(URPShaderConstant.ZWRITE, 1);
                material.SetInt(URPShaderConstant.ZWRITE_CONTROL, 0);// 0で良さそう
                material.SetInt(URPShaderConstant.ZTEST, (int)CompareFunction.Less);
                material.SetFloat(URPShaderConstant.CULL, (float)_renderFace);

                material.DisableKeyword(URPShaderConstant.ALPHATEST_ON);
                material.DisableKeyword(URPShaderConstant.ALPHABLEND_ON);
                material.DisableKeyword(URPShaderConstant.ALPHAPREMULTIPLY_ON);

                material.renderQueue = (int)RenderQueue.Geometry;
            }
            else if (_blendMode == BlendMode_MToon.Cutout)
            {
                material.SetOverrideTag(URPShaderConstant.RENDER_TYPE, "TransparentCutout");

                material.SetFloat(URPShaderConstant.SURFACE, (float)SurfaceType.Opaque);
                //material.SetFloat(URPShaderConstant.BLEND_MODE, (float)BlendMode.Alpha);
                material.SetFloat(URPShaderConstant.ALPHA_CLIP, 1);
                material.SetInt(URPShaderConstant.SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt(URPShaderConstant.DST_BLEND, (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt(URPShaderConstant.ZWRITE, 1);
                material.SetInt(URPShaderConstant.ZWRITE_CONTROL, 0);// 0で良さそう
                material.SetInt(URPShaderConstant.ZTEST, (int)CompareFunction.Less);
                material.SetFloat(URPShaderConstant.CULL, (float)_renderFace);

                material.EnableKeyword(URPShaderConstant.ALPHATEST_ON);
                material.DisableKeyword(URPShaderConstant.ALPHABLEND_ON);
                material.DisableKeyword(URPShaderConstant.ALPHAPREMULTIPLY_ON);

                material.renderQueue = (int)RenderQueue.AlphaTest;
            }
            else if (_blendMode == BlendMode_MToon.Transparent)
            {
                material.SetOverrideTag(URPShaderConstant.RENDER_TYPE, "Transparent");

                material.SetFloat(URPShaderConstant.SURFACE, (float)SurfaceType.Transparent);
                material.SetFloat(URPShaderConstant.BLEND_MODE, (float)BlendMode.Alpha);
                material.SetFloat(URPShaderConstant.ALPHA_CLIP, 0);
                material.SetInt(URPShaderConstant.SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt(URPShaderConstant.DST_BLEND, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt(URPShaderConstant.ZWRITE, 0);
                material.SetInt(URPShaderConstant.ZWRITE_CONTROL, 0);// 0で良さそう
                material.SetInt(URPShaderConstant.ZTEST, (int)CompareFunction.LessEqual);
                material.SetFloat(URPShaderConstant.CULL, (float)CullMode.Off);//強制的に両面にしてみる

                material.DisableKeyword(URPShaderConstant.ALPHATEST_ON);
                material.DisableKeyword(URPShaderConstant.ALPHABLEND_ON);
                material.DisableKeyword(URPShaderConstant.ALPHAPREMULTIPLY_ON);

                material.renderQueue = (int)RenderQueue.Transparent;
            }
            // NOTE: 本来すべき設定と違う気がする..
            else if (_blendMode == BlendMode_MToon.TransparentWithZWrite)
            {
                material.SetOverrideTag(URPShaderConstant.RENDER_TYPE, "Transparent");

                material.SetFloat(URPShaderConstant.SURFACE, (float)SurfaceType.Transparent);
                material.SetFloat(URPShaderConstant.BLEND_MODE, (float)BlendMode.Alpha);
                material.SetFloat(URPShaderConstant.ALPHA_CLIP, 0);
                material.SetInt(URPShaderConstant.SRC_BLEND, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt(URPShaderConstant.DST_BLEND, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt(URPShaderConstant.ZWRITE, 1);// 0ダメ
                material.SetInt(URPShaderConstant.ZWRITE_CONTROL, 0);// 0で良さそう
                material.SetInt(URPShaderConstant.ZTEST, (int)CompareFunction.LessEqual);
                material.SetFloat(URPShaderConstant.CULL, (float)CullMode.Off);//強制的に両面にしてみる

                material.DisableKeyword(URPShaderConstant.ALPHATEST_ON);// EnableKeywordだと目のハイライトが何故..
                material.EnableKeyword(URPShaderConstant.ALPHABLEND_ON);
                material.DisableKeyword(URPShaderConstant.ALPHAPREMULTIPLY_ON);

                //どっちでもよさげ？
                material.renderQueue = (int)RenderQueue.Transparent;
                //material.renderQueue = (int)RenderQueue.Transparent + 1;
            }
        }


        void IMaterialConverter.Dispose()
        {
            
        }
    }
}