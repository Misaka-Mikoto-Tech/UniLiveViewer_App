using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    //������URP�pshader�������s�v
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
        public List<Material> materials_Base { get; private set; } = new List<Material>();//���Z�b�g�p�Ɏ擾

        //�u���V�F�[�_�[
        [SerializeField] private Shader targetShader;
        [SerializeField] private Shader replaceShader;

        /// <summary>
        /// 
        /// </summary>
        public void InitMaterials()
        {
            //UniGLTF/Unlit�͖��Ȃ�����
            targetShader = Shader.Find("Universal Render Pipeline/Lit");
            replaceShader = Shader.Find("Universal Render Pipeline/Unlit");

            //�SMaterial�����擾
            GetMaterials(transform);

            //�u���Ώۃ}�e���A��������
            if (materials.Count > 0)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    //�V�F�[�_�[�������ւ���
                    materials[i].shader = replaceShader;

                    //���O���������ۂ���(�G)
                    if (materials[i].name.Contains("transparent"))
                    {
                        //�����������Ή�
                        materials[i].SetFloat("_Surface", (float)SurfaceType.Transparent);
                        materials[i].SetFloat("_Blend", (float)BlendMode.Alpha);
                    }

                    //����
                    SetupMaterialBlendMode(materials[i]);
                }
            }
        }

        /// <summary>
        /// �X�L�����b�V�������_�[����SMaterial���擾
        /// </summary>
        /// <param name="parent"></param>
        private void GetMaterials(Transform parent)
        {
            foreach (Transform child in parent)
            {
                //�{�[���̊K�w�\���͖���
                if (child.name.Contains("root")) continue;
                if (child.name == "secondary") return;

                if (child.GetComponent<SkinnedMeshRenderer>() != null)
                {
                    //�X�L�����b�V�����擾
                    var skin = child.GetComponent<SkinnedMeshRenderer>();
                    //layer�ݒ�
                    if (child.name.Contains("eye") && child.name.Contains("face"))
                    {
                        //�ڂ��ɃA�E�g���C���͎c�O�Ȋ����ɂȂ�₷���̂Őݒ肵�Ȃ�
                    }
                    else
                    {
                        skin.gameObject.layer = gameObject.layer;//���C���[�𑵂���
                    }
                    skinMesh.Add(skin);

                    if (child.name.Contains("_headless"))
                    {
                        //�s�v�Ȃ̂Ŗ��������Ă����A�Ǘ����Ȃ�
                        child.gameObject.SetActive(false);
                    }
                    else
                    {
                        foreach (var mat in skin.sharedMaterials)//���̃}�e���A�����擾
                        {
                            //�^�[�Q�b�g�Ȃ烊�X�g�ɒǉ�
                            if (mat.shader == targetShader)
                            {
                                materials_Base.Add(skin.sharedMaterial);
                            }
                        }
                        foreach (var mat in skin.materials)//�������ꂽ�}�e���A�����擾
                        {
                            //�^�[�Q�b�g�Ȃ烊�X�g�ɒǉ�
                            if (mat.shader == targetShader)
                            {
                                materials.Add(mat);
                            }
                        }
                    }
                }
                //�ċA
                GetMaterials(child);
            }
        }

        /// <summary>
        /// �}�e���A���̐ݒ�l��ύX�O�ɖ߂�
        /// </summary>
        public void ResetMaterials()
        {
            if (materials.Count != materials_Base.Count) return;

            for (int i = 0; i < materials.Count; i++)
            {
                //�x�[�X����ݒ�l���R�s�[
                materials[i].SetFloat("_Surface", (float)materials_Base[i].GetFloat("_Surface"));
                materials[i].SetFloat("_Blend", (float)materials_Base[i].GetFloat("_Blend"));
                materials[i].SetFloat("_Cull", (float)materials_Base[i].GetFloat("_Cull"));
                materials[i].color = materials_Base[i].color;

                //����
                SetupMaterialBlendMode(materials[i]);
            }
        }

        /// <summary>
        /// �T�[�t�F�X�^�C�v��ݒ�
        /// </summary>
        /// <param name="current"></param>
        /// <param name="type"></param>
        public void SetSurface(int current, SurfaceType type)
        {
            //SurfaceType
            materials[current].SetFloat("_Surface", (float)type);

            //����
            SetupMaterialBlendMode(materials[current]);
        }

        /// <summary>
        /// �u�����h���[�h��ݒ�
        /// </summary>
        /// <param name="current"></param>
        /// <param name="mode"></param>
        public void SetBlendMode(int current, BlendMode mode)
        {
            //BlendMode
            materials[current].SetFloat("_Blend", (float)mode);

            //����
            SetupMaterialBlendMode(materials[current]);
        }

        /// <summary>
        /// �`��ʂ�ݒ�
        /// </summary>
        /// <param name="current"></param>
        /// <param name="render"></param>
        public void SetRenderFace(int current, RenderFace render)
        {
            //RenderFace
            materials[current].SetFloat("_Cull", (float)render);

            //����
            //SetupMaterialBlendMode(materials[current]);
        }

        /// <summary>
        /// �J���[�̃A���t�@�̂ݐݒ�
        /// </summary>
        /// <param name="current"></param>
        /// <param name="alpha"></param>
        public void SetColor_Transparent(int current, float alpha)
        {
            //��������
            Color col = materials[current].color;
            col.a = alpha;
            materials[current].color = col;
        }

        /// <summary>
        /// �Q�l���Ƃ��������̂܂܃R�s�y
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