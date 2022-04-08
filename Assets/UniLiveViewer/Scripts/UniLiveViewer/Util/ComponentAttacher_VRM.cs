using System.Collections.Generic;
using UnityEngine;
using VRM;
using UniHumanoid;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniLiveViewer 
{
    //TODO:�G���[���O���炢�o��
    public class ComponentAttacher_VRM : MonoBehaviour
    {
        [SerializeField] private RuntimeAnimatorController aniConPrefab = null;
        [SerializeField] private GameObject LipSyncPrefab = null;
        [SerializeField] private GameObject FaceSyncPrefab = null;
        [SerializeField] private CharaInfoData charaInfoDataPrefab;
        [SerializeField] private AttachPoint attachPointPrefab;
        [SerializeField] private GameObject lowShadowPrefab;
        [SerializeField] private GameObject guidePrefab;

        private VRMMeta meta;
        private GameObject targetVRM;
        private Animator animator;
        private CharaController charaCon;

        public async UniTask Init(Transform _targetVRM, VRMTouchColliders touchCollider, CancellationToken token)
        {
            //VRM�m�F
            targetVRM = _targetVRM.gameObject;
            transform.parent = targetVRM.transform;
            meta = targetVRM.GetComponent<VRMMeta>();
            if (meta == null) return;

            animator = targetVRM.GetComponent<Animator>();
            charaCon = targetVRM.GetComponent<CharaController>();

            await UniTask.WhenAll(
                CustomizeComponent_Standard(token),
                CustomizeComponent_VRM(touchCollider, token));
        }

        private async UniTask CustomizeComponent_Standard(CancellationToken token)
        {
            try
            {
                //Animation�֘A�̒���
                animator.runtimeAnimatorController = aniConPrefab;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                animator.applyRootMotion = true;

                //�e�I�u�W�F�̒ǉ�
                var lowShadow = Instantiate(lowShadowPrefab);
                lowShadow.transform.parent = animator.GetBoneTransform(HumanBodyBones.Hips);
                lowShadow.transform.localPosition = Vector3.zero;
                lowShadow = null;

                //�R���C�_�[�̒ǉ�
                var capcol = targetVRM.gameObject.AddComponent<CapsuleCollider>();
                capcol.center = new Vector3(0, 0.8f, 0);
                capcol.radius = 0.25f;
                capcol.height = 1.5f;

                //���W�b�g�{�f�B�̒ǉ�
                var rb = targetVRM.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.isKinematic = true;
                rb.useGravity = false;

                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //�͂݊֘A�̒ǉ�
                targetVRM.AddComponent<MeshRenderer>();
                targetVRM.AddComponent<OVRGrabbable_Custom>();

                //�K�C�h�̒ǉ�
                targetVRM.AddComponent<MeshGuide>().guidePrefab = guidePrefab;

                //�A�^�b�`�|�C���g�̒ǉ�
                targetVRM.AddComponent<AttachPointGenerator>().anchorPointPrefab = attachPointPrefab;

                //����\��̒ǉ�
                //var specialFacial = vrmModel.AddComponent<SpecialFacial>();
                //specialFacial.SetAudioClip_VRM(specialFaceAudioClip);

                //�L�����Z���m�F
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// VRM�̃R���|�[�l���g���J�X�^�}�C�Y����
        /// </summary>
        private async UniTask CustomizeComponent_VRM(VRMTouchColliders touchCollider, CancellationToken token)
        {
            try
            {
                //�s�v�ȃX�N���v�g���~
                targetVRM.GetComponent<HumanPoseTransfer>().enabled = false;
                //vrmModel.GetComponent<Blinker>().enabled = false;

                var blendShapeProxy = targetVRM.GetComponent<VRMBlendShapeProxy>();

                //���b�v�V���N�̒ǉ�
                LipSyncController.Instantiate(LipSyncPrefab, charaCon, blendShapeProxy);

                //�t�F�C�X�V���N�̒ǉ�
                FacialSyncController.Instantiate(FaceSyncPrefab, charaCon, blendShapeProxy);

                //VMD�v���C���[�ǉ�(�eSync�n�̌�ɒǉ�����)
                targetVRM.AddComponent<VMDPlayer>();
                //ScriptableObject�ǉ�
                charaCon.charaInfoData = Instantiate(charaInfoDataPrefab);
                //charaCon.charaInfoData = charaInfoDataPrefab;

                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //�����֘A�̒���
                var lookAtHead = targetVRM.GetComponent<VRMLookAtHead_Custom>();
                var lookAtCon = targetVRM.AddComponent<LookAtController>();
                lookAtHead.Target = GameObject.FindGameObjectWithTag("MainCamera").transform;
                lookAtCon.SetVRMComponent(animator, charaCon, lookAtHead.Target);
                lookAtHead.UpdateType = UpdateType.LateUpdate;
                if (targetVRM.GetComponent<VRMLookAtBoneApplyer_Custom>() != null)
                {
                    charaCon.charaInfoData.charaType = CharaInfoData.CHARATYPE.VRM_Bone;
                    lookAtCon.VRMLookAtEye_Bone = targetVRM.GetComponent<VRMLookAtBoneApplyer_Custom>();
                }
                if (targetVRM.GetComponent<VRMLookAtBlendShapeApplyer_Custom>() != null)
                {
                    charaCon.charaInfoData.charaType = CharaInfoData.CHARATYPE.VRM_BlendShape;
                    lookAtCon.VRMLookAtEye_UV = targetVRM.GetComponent<VRMLookAtBlendShapeApplyer_Custom>();
                }

                string name = meta.Meta.Title;
                if (name != "") charaCon.charaInfoData.viewName = name;
                else charaCon.charaInfoData.viewName = targetVRM.name;

                //�����狹�{�[�������
                Transform headAnchor = targetVRM.GetComponent<VRMLookAtHead_Custom>().Head.transform;

                //�h�ꃂ�m����
                List<VRMSpringBoneColliderGroup> colliderList = new List<VRMSpringBoneColliderGroup>();//�����p
                var SpringBone = targetVRM.transform.Find("secondary").GetComponents<VRMSpringBone>();
                for (int i = 0; i < SpringBone.Length; i++)
                {
                    //�e�z������X�g�ɓ���
                    if (SpringBone[i].ColliderGroups != null && SpringBone[i].ColliderGroups.Length > 0)
                    {
                        colliderList.AddRange(SpringBone[i].ColliderGroups);//�����R���C�_�[
                        colliderList.AddRange(touchCollider.colliders);//�ǉ��R���C�_�[(PlayerHand)                                                                                                                                                                            
                        //���X�g����z��ɖ߂�
                        SpringBone[i].ColliderGroups = colliderList.ToArray();
                        colliderList.Clear();
                    }
                    else
                    {
                        //������΂��̂܂܃Z�b�g
                        SpringBone[i].ColliderGroups = touchCollider.colliders;
                    }
                    //�L�����R���ɓo�^
                    charaCon.springBoneList.Add(SpringBone[i]);

                    SpringBone[i].enabled = true;
                }

                //�L�����Z���m�F
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                Debug.Log("Custom�G���[");
                throw;
            }
        }
    }

}
