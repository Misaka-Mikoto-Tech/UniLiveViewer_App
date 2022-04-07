using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class DecorationItemInfo : MonoBehaviour
    {
        public string[] itemName = new string[2] { "ItemName", "�A�C�e����" };
        public string[] flavorText = new string[2] { "Unremarkable item", "���̕ϓN���Ȃ��A�C�e��" };
        public Texture2D[] texs = new Texture2D[0];
        public RenderInfo[] renderInfo = new RenderInfo[1];

        // Start is called before the first frame update
        void Start()
        {
            try
            {
                foreach (var info in renderInfo)
                {
                    for (int i = 0; i < info._renderer.materials.Length; i++)
                    {
                        try
                        {
                            //�����_�[�Ƃ��̃}�e���A���v���p�e�B���擾
                            info.materialPropertyBlock.Add(new MaterialPropertyBlock());
                            info._renderer.GetPropertyBlock(info.materialPropertyBlock[i]);
                            //�����J���[���擾���Ă���
                            //TODO:�������C�g�}�b�v�̃A�C�e�����ƃG���[�A�����꒼��
                            info.initColor.Add(info._renderer.materials[i].GetColor("_BaseColor"));
                        }
                        catch
                        {
                            Debug.Log("_BaseColor������܂���B���C�g�}�b�v�����Ȃ��z");
                        }
                    }

                    //�}�e���A���I�u�W�F�̏Փ˃C�x���g
                    if (info.itemCollisionChecker) info.itemCollisionChecker.OnTrigger += SetTexture;
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// �w��e�N�X�`���ɕύX
        /// </summary>
        public void SetTexture(ItemCollisionChecker parts, Collider other)
        {
            var newTextur = other.GetComponent<MeshRenderer>().materials[0].GetTexture("_BaseMap");

            foreach (var info in renderInfo)
            {
                //���ʂ̓���
                if (info.itemCollisionChecker == parts)
                {
                    //�Ώۃ��b�V���̑S�}�e���A����Tex��u��
                    for (int i = 0; i < info.materialPropertyBlock.Count; i++)
                    {
                        info.materialPropertyBlock[i].SetTexture("_BaseMap", newTextur);
                        info._renderer.SetPropertyBlock(info.materialPropertyBlock[i]);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// �����J���[�ɖ߂�
        /// </summary>
        /// <param name="renderIndex"></param>
        /// <param name="materialIndex"></param>
        public void SetInitColor(int renderIndex, int materialIndex)
        {
            if (renderInfo.Length < renderIndex) return;
            if (renderInfo[renderIndex].materialPropertyBlock.Count < materialIndex) return;
            RenderInfo target = renderInfo[renderIndex];

            target.materialPropertyBlock[materialIndex].SetColor("_BaseColor", target.initColor[materialIndex]);
            target._renderer.SetPropertyBlock(target.materialPropertyBlock[materialIndex]);
        }

        /// <summary>
        /// �w��J���[�ɕύX
        /// </summary>
        /// <param name="renderIndex"></param>
        /// <param name="materialIndex"></param>
        /// <param name="setColor"></param>
        public void SetColor(int renderIndex, int materialIndex, Color setColor)
        {
            if (renderInfo.Length < renderIndex) return;
            if (renderInfo[renderIndex].materialPropertyBlock.Count < materialIndex) return;
            RenderInfo target = renderInfo[renderIndex];

            target.materialPropertyBlock[materialIndex].SetColor("_BaseColor", setColor);
            target._renderer.SetPropertyBlock(target.materialPropertyBlock[materialIndex]);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < renderInfo.Length; i++)
            {
                for (int j = 0; j < renderInfo[i]._renderer.materials.Length; j++)
                {
                    Destroy(renderInfo[i]._renderer.materials[j]);
                }
            }
            renderInfo = null;
        }
    }

    [System.Serializable]
    public class RenderInfo
    {
        public Renderer _renderer;
        public ItemCollisionChecker itemCollisionChecker;//�}�e���A���I�u�W�F�Ƃ̐ڐG�ŐF��ς���ꍇ�͎w�肷��
        public string[] partsName = new string[2] { "partsName", "���ʂ̖��O" };//���̂Ƃ����g�p
        public List<MaterialPropertyBlock> materialPropertyBlock = new List<MaterialPropertyBlock>();
        public List<Color> initColor = new List<Color>();
    }
}