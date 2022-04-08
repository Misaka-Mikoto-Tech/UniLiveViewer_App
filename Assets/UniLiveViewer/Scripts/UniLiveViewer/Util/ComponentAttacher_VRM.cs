using System.Collections.Generic;
using UnityEngine;
using VRM;
using UniHumanoid;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniLiveViewer 
{
    //TODO:エラーログくらい出せ
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
            //VRM確認
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
                //Animation関連の調整
                animator.runtimeAnimatorController = aniConPrefab;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                animator.applyRootMotion = true;

                //影オブジェの追加
                var lowShadow = Instantiate(lowShadowPrefab);
                lowShadow.transform.parent = animator.GetBoneTransform(HumanBodyBones.Hips);
                lowShadow.transform.localPosition = Vector3.zero;
                lowShadow = null;

                //コライダーの追加
                var capcol = targetVRM.gameObject.AddComponent<CapsuleCollider>();
                capcol.center = new Vector3(0, 0.8f, 0);
                capcol.radius = 0.25f;
                capcol.height = 1.5f;

                //リジットボディの追加
                var rb = targetVRM.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.isKinematic = true;
                rb.useGravity = false;

                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //掴み関連の追加
                targetVRM.AddComponent<MeshRenderer>();
                targetVRM.AddComponent<OVRGrabbable_Custom>();

                //ガイドの追加
                targetVRM.AddComponent<MeshGuide>().guidePrefab = guidePrefab;

                //アタッチポイントの追加
                targetVRM.AddComponent<AttachPointGenerator>().anchorPointPrefab = attachPointPrefab;

                //特殊表情の追加
                //var specialFacial = vrmModel.AddComponent<SpecialFacial>();
                //specialFacial.SetAudioClip_VRM(specialFaceAudioClip);

                //キャンセル確認
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// VRMのコンポーネントをカスタマイズする
        /// </summary>
        private async UniTask CustomizeComponent_VRM(VRMTouchColliders touchCollider, CancellationToken token)
        {
            try
            {
                //不要なスクリプトを停止
                targetVRM.GetComponent<HumanPoseTransfer>().enabled = false;
                //vrmModel.GetComponent<Blinker>().enabled = false;

                var blendShapeProxy = targetVRM.GetComponent<VRMBlendShapeProxy>();

                //リップシンクの追加
                LipSyncController.Instantiate(LipSyncPrefab, charaCon, blendShapeProxy);

                //フェイスシンクの追加
                FacialSyncController.Instantiate(FaceSyncPrefab, charaCon, blendShapeProxy);

                //VMDプレイヤー追加(各Sync系の後に追加する)
                targetVRM.AddComponent<VMDPlayer>();
                //ScriptableObject追加
                charaCon.charaInfoData = Instantiate(charaInfoDataPrefab);
                //charaCon.charaInfoData = charaInfoDataPrefab;

                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //注視関連の調整
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

                //頭から胸ボーンを特定
                Transform headAnchor = targetVRM.GetComponent<VRMLookAtHead_Custom>().Head.transform;

                //揺れモノ調整
                List<VRMSpringBoneColliderGroup> colliderList = new List<VRMSpringBoneColliderGroup>();//統合用
                var SpringBone = targetVRM.transform.Find("secondary").GetComponents<VRMSpringBone>();
                for (int i = 0; i < SpringBone.Length; i++)
                {
                    //各配列をリストに統合
                    if (SpringBone[i].ColliderGroups != null && SpringBone[i].ColliderGroups.Length > 0)
                    {
                        colliderList.AddRange(SpringBone[i].ColliderGroups);//既存コライダー
                        colliderList.AddRange(touchCollider.colliders);//追加コライダー(PlayerHand)                                                                                                                                                                            
                        //リストから配列に戻す
                        SpringBone[i].ColliderGroups = colliderList.ToArray();
                        colliderList.Clear();
                    }
                    else
                    {
                        //無ければそのままセット
                        SpringBone[i].ColliderGroups = touchCollider.colliders;
                    }
                    //キャラコンに登録
                    charaCon.springBoneList.Add(SpringBone[i]);

                    SpringBone[i].enabled = true;
                }

                //キャンセル確認
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                Debug.Log("Customエラー");
                throw;
            }
        }
    }

}
