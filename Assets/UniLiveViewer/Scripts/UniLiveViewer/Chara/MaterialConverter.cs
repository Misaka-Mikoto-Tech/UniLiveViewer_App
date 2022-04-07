using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    //ちゃんとURP用shader作ったら不要
    public class MaterialConverter : MonoBehaviour
    {
        public enum SurfaceType
        {
            Opaque,
            Transparent
        }
        public enum BlendMode
        {
            Alpha,
            Premultiply,
            Additive,
            Multiply
        }
        public enum RenderFace
        {
            Both,
            Back,
            Front
        }

        public List<SkinnedMeshRenderer> skinMesh { get; private set; } = new List<SkinnedMeshRenderer>();
        public List<Material> materials { get; private set; } = new List<Material>();
        public List<Material> materials_Base { get; private set; } = new List<Material>();//リセット用に取得

        //置換シェーダー
        [SerializeField] private Shader targetShader;
        [SerializeField] private Shader replaceShader;

        /// <summary>
        /// 
        /// </summary>
        public void InitMaterials()
        {
            //UniGLTF/Unlitは問題なさそう
            targetShader = Shader.Find("Universal Render Pipeline/Lit");
            replaceShader = Shader.Find("Universal Render Pipeline/Unlit");

            //全Material情報を取得
            GetMaterials(transform);

            //置換対象マテリアルを処理
            if (materials.Count > 0)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    //シェーダーを差し替える
                    materials[i].shader = replaceShader;

                    //名前が透明っぽいか(雑)
                    if (materials[i].name.Contains("transparent"))
                    {
                        //透明化だけ対応
                        materials[i].SetFloat("_Surface", (float)SurfaceType.Transparent);
                        materials[i].SetFloat("_Blend", (float)BlendMode.Alpha);
                    }

                    //調整
                    SetupMaterialBlendMode(materials[i]);
                }
            }
        }

        /// <summary>
        /// スキンメッシュレンダーから全Materialを取得
        /// </summary>
        /// <param name="parent"></param>
        private void GetMaterials(Transform parent)
        {
            foreach (Transform child in parent)
            {
                //ボーンの階層構造は無視
                if (child.name.Contains("root")) continue;
                if (child.name == "secondary") return;

                if (child.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    //スキンメッシュを取得
                    var skin = child.GetComponent<SkinnedMeshRenderer>();
                    //layer設定
                    if (child.name.Contains("eye") && child.name.Contains("face"))
                    {
                        //目や顔にアウトラインは残念な感じになりやすいので設定しない
                    }
                    else
                    {
                        skin.gameObject.layer = gameObject.layer;//レイヤーを揃える
                    }
                    skinMesh.Add(skin);

                    if (child.name.Contains("_headless"))
                    {
                        //不要なので無効化しておく、管理しない
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        foreach (var mat in skin.sharedMaterials)//元のマテリアルを取得
                        {
                            //ターゲットならリストに追加
                            if (mat.shader == targetShader)
                            {
                                materials_Base.Add(skin.sharedMaterial);
                            }
                        }
                        foreach (var mat in skin.materials)//複製されたマテリアルを取得
                        {
                            //ターゲットならリストに追加
                            if (mat.shader == targetShader)
                            {
                                materials.Add(mat);
                            }
                        }
                    }
                }
                //再帰
                GetMaterials(child);
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
        public void SetSurface(int current, SurfaceType type)
        {
            //SurfaceType
            materials[current].SetFloat("_Surface", (float)type);

            //調整
            SetupMaterialBlendMode(materials[current]);
        }

        /// <summary>
        /// ブレンドモードを設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="mode"></param>
        public void SetBlendMode(int current, BlendMode mode)
        {
            //BlendMode
            materials[current].SetFloat("_Blend", (float)mode);

            //調整
            SetupMaterialBlendMode(materials[current]);
        }

        /// <summary>
        /// 描画面を設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="render"></param>
        public void SetRenderFace(int current, RenderFace render)
        {
            //RenderFace
            materials[current].SetFloat("_Cull", (float)render);

            //調整
            //SetupMaterialBlendMode(materials[current]);
        }

        /// <summary>
        /// カラーのアルファのみ設定
        /// </summary>
        /// <param name="current"></param>
        /// <param name="alpha"></param>
        public void SetColor_Transparent(int current, float alpha)
        {
            //透明調整
            Color col = materials[current].color;
            col.a = alpha;
            materials[current].color = col;
        }

        /// <summary>
        /// 参考元というかそのままコピペ
        /// ttps://answers.unity.com/questions/1608815/change-surface-type-with-lwrp.html
        /// </summary>
        /// <param name="material"></param>
        void SetupMaterialBlendMode(Material material)
        {
            //if (material == null)
            //throw new ArgumentNullException("material");
            bool alphaClip = material.GetFloat("_AlphaClip") == 1;
            if (alphaClip)
                material.EnableKeyword("_ALPHATEST_ON");
            else
                material.DisableKeyword("_ALPHATEST_ON");
            SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
            if (surfaceType == 0)
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
                BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
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