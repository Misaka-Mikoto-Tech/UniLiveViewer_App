using System.Collections.Generic;
using UnityEngine;
using VRM;
using UniHumanoid;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

namespace UniLiveViewer 
{
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

        private List<VRMSpringBone> vrmSpringBones = new List<VRMSpringBone>();

        public void Init(Transform _targetVRM)
        {
            try
            {
                targetVRM = _targetVRM.gameObject;
                transform.parent = targetVRM.transform;
                meta = targetVRM.GetComponent<VRMMeta>();
                if (meta == null) return;

                //非同期だと乱れが生じるのでinstance化まで動かさない
                vrmSpringBones.AddRange(targetVRM.transform.Find("secondary").GetComponents<VRMSpringBone>());

                foreach (var e in vrmSpringBones)
                {
                    e.enabled = false;
                }

                //Meshが消える対策
                var meshs = targetVRM.GetComponentsInChildren<SkinnedMeshRenderer>();
                //Bounds bounds;
                foreach (var mesh in meshs)
                {
                    //上手くいかない
                    //bounds = mesh.bounds;
                    //bounds.Expand(Vector3.one);
                    //mesh.bounds = bounds;

                    //良くないがこれで
                    mesh.updateWhenOffscreen = true;
                }

                animator = targetVRM.GetComponent<Animator>();
                charaCon = targetVRM.GetComponent<CharaController>();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw new Exception("Attacher initialize");
            }
        }
        public async UniTask Attachment(VRMTouchColliders touchCollider, CancellationToken token)
        {
            try
            {
                await UniTask.WhenAll(
                CustomizeComponent_Standard(token),
                CustomizeComponent_VRM(touchCollider, token));
            }
            catch
            {
                throw;
            }
            finally
            {
                Destroy(gameObject);
            }
        }

        private async UniTask CustomizeComponent_Standard(CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
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
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw new Exception("Standard Attacher");
            }
        }

        /// <summary>
        /// VRMのコンポーネントをカスタマイズする
        /// </summary>
        private async UniTask CustomizeComponent_VRM(VRMTouchColliders touchCollider, CancellationToken token)
        {
            try
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                //不要なスクリプトを停止
                targetVRM.GetComponent<HumanPoseTransfer>().enabled = false;
                //vrmModel.GetComponent<Blinker>().enabled = false;

                var blendShapeProxy = targetVRM.GetComponent<VRMBlendShapeProxy>();

                //ScriptableObject追加
                charaCon.charaInfoData = Instantiate(charaInfoDataPrefab);

                //リップシンクの追加(ScriptableObject後)
                LipSyncController.Instantiate(LipSyncPrefab, charaCon, blendShapeProxy);

                //フェイスシンクの追加(ScriptableObject後)
                FacialSyncController.Instantiate(FaceSyncPrefab, charaCon, blendShapeProxy);

                //VMDプレイヤー追加(各Sync系の後)
                targetVRM.AddComponent<VMDPlayer_Custom>();

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

                //揺れモノ調整
                List<VRMSpringBoneColliderGroup> colliderList = new List<VRMSpringBoneColliderGroup>();//統合用
                for (int i = 0; i < vrmSpringBones.Count; i++)
                {
                    //各配列をリストに統合
                    if (vrmSpringBones[i].ColliderGroups != null && vrmSpringBones[i].ColliderGroups.Length > 0)
                    {
                        colliderList.AddRange(vrmSpringBones[i].ColliderGroups);//既存コライダー
                        colliderList.AddRange(touchCollider.colliders);//追加コライダー(PlayerHand)                                                                                                                                                                              
                        //リストから配列に戻す
                        vrmSpringBones[i].ColliderGroups = colliderList.ToArray();
                        colliderList.Clear();
                        //登録
                        charaCon.springBoneList.Add(vrmSpringBones[i]);
                    }
                    else
                    {
                        //そのまま追加
                        vrmSpringBones[i].ColliderGroups = touchCollider.colliders;
                        //登録
                        charaCon.springBoneList.Add(vrmSpringBones[i]);
                    }
                }

                //キャンセル確認
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                throw new Exception("VRM Attacher");
            }
        }
    }

}
