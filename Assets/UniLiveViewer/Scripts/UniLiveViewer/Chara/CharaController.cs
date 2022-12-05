using System.Collections.Generic;
using UnityEngine;
using VRM;
using NanaCiel;

namespace UniLiveViewer
{
    public class CharaController : MonoBehaviour
    {
        public enum CHARASTATE
        {
            NULL = 0,
            MINIATURE,
            HOLD,
            ON_CIRCLE,
            FIELD,
        }
        public enum ANIMATIONMODE
        {
            CLIP = 0,
            VMD,
        }

        public CHARASTATE GetCharaState => charaState;
        [SerializeField] CHARASTATE charaState = CHARASTATE.NULL;
        public ANIMATIONMODE animationMode = ANIMATIONMODE.CLIP;
        public Animator GetAnimator => _animator;
        Animator _animator;

        public List<VRMSpringBone> springBoneList = new List<VRMSpringBone>();//揺れもの接触判定用

        //Sync
        public bool isLipSyncUpdate = false;
        public bool isFacialSyncUpdate = false;
        public ILipSync _lipSync;
        public IFacialSync _facialSync;

        //LookAt
        public LookAtBase _lookAt;
        public bool isHeadLookAtUpdate = false;
        public bool isEyeLookAtUpdate = false;
        public IHeadLookAt _headLookAt;
        public IEyeLookAt _eyeLookAt;
        public ILookAtVRM _lookAtVRM;

        public CharaInfoData charaInfoData;
        float _customScalar = 0;
        //現状VRM専用
        public IReadOnlyList<SkinnedMeshRenderer> GetSkinnedMeshRenderers;
        public IReadOnlyList<SkinnedMeshRenderer> GetMorphSkinnedMeshRenderers;
        public void SetSkinnedMeshRenderers(IReadOnlyList<SkinnedMeshRenderer> skins)
        {
            GetSkinnedMeshRenderers = skins;
            GetMorphSkinnedMeshRenderers = skins.GetMorphSkinnedMeshRenderer();
        }

        [Header("＜TimeLine自動管理＞")]
        public string bindTrackName = "";
        public RuntimeAnimatorController keepRunAnime;//アニメーション無効化の際に直前状態をキープ
        public AnimationClip keepHandL_Anime;//手アニメーション(握りから戻す際に必要)
        public AnimationClip keepHandR_Anime;//手アニメーション(握りから戻す際に必要)
        [SerializeField] private Transform overrideAnchor = null;//座標の上書き用
        

        public float CustomScalar
        {
            get { return _customScalar; }
            set
            {
                _customScalar = Mathf.Clamp(value, 0.25f, 20.0f);
                transform.localScale = Vector3.one * _customScalar;
            }
        }

        void Awake()
        {
            _animator = transform.GetComponent<Animator>();

            //VRMはまだcharaInfoData生成前
            if (charaInfoData && charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
            {
                _lipSync = transform.GetComponentInChildren<ILipSync>();
                _facialSync = transform.GetComponentInChildren<IFacialSync>();
                _lookAt = transform.GetComponent<LookAtBase>();
                _headLookAt = transform.GetComponent<IHeadLookAt>();
                _eyeLookAt = transform.GetComponent<IEyeLookAt>();
            }

            _customScalar = SystemInfo.userProfile.InitCharaSize;
        }

        public void InitVRMSync(LipSync_VRM lipSync, FacialSync_VRM facialSync, VRMBlendShapeProxy blendShapeProxy)
        {
            lipSync.transform.name = "LipSyncController";
            lipSync.transform.parent = transform;
            lipSync.vrmBlendShape = blendShapeProxy;
            _lipSync = lipSync;

            facialSync.transform.name = "FaceSyncController";
            facialSync.transform.parent = transform;
            facialSync.vrmBlendShape = blendShapeProxy;
            _facialSync = facialSync;
        }

        public void InitLookAtController()
        {
            if (charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                gameObject.AddComponent<NormalizedBoneGenerator>();

                //VRMボーン
                if (GetComponent<VRMLookAtBoneApplyer_Custom>() != null)
                {
                    var lookAt = gameObject.AddComponent<LookAt_VRMBone>();
                    _lookAt = transform.GetComponent<LookAtBase>();
                    _headLookAt = transform.GetComponent<IHeadLookAt>();
                    _eyeLookAt = transform.GetComponent<IEyeLookAt>();
                    _lookAtVRM = transform.GetComponent<ILookAtVRM>();

                    charaInfoData.charaType = CharaInfoData.CHARATYPE.VRM_Bone;
                    ((LookAt_VRMBone)_lookAtVRM)._vrmEyeApplyer = GetComponent<VRMLookAtBoneApplyer_Custom>();

                    var lookAtHead = transform.GetComponent<VRMLookAtHead_Custom>();
                    lookAtHead.Target = lookAt.GetLookAtTarget();
                    lookAtHead.UpdateType = UpdateType.LateUpdate;
                }
                //VRMブレンドシェイプ
                else if (GetComponent<VRMLookAtBlendShapeApplyer_Custom>() != null)
                {
                    var lookAt = gameObject.AddComponent<LookAt_VRMBlendShape>();
                    _lookAt = transform.GetComponent<LookAtBase>();
                    _headLookAt = transform.GetComponent<IHeadLookAt>();
                    _eyeLookAt = transform.GetComponent<IEyeLookAt>();
                    _lookAtVRM = transform.GetComponent<ILookAtVRM>();

                    charaInfoData.charaType = CharaInfoData.CHARATYPE.VRM_BlendShape;
                    ((LookAt_VRMBlendShape)_lookAtVRM)._vrmEyeApplyer = GetComponent<VRMLookAtBlendShapeApplyer_Custom>();

                    var lookAtHead = transform.GetComponent<VRMLookAtHead_Custom>();
                    lookAtHead.Target = lookAt.GetLookAtTarget();
                    lookAtHead.UpdateType = UpdateType.LateUpdate;
                }
                //UVはそもそも用意されて無いのだ
            }
        }

        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="setState"></param>
        /// <param name="overrideTarget">対象位置に座標を合わせる</param>
        public void SetState(CHARASTATE setState, Transform overrideTarget)
        {
            Vector3 globalScale = Vector3.zero;

            overrideAnchor = overrideTarget;
            charaState = setState;
            switch (charaState)
            {
                case CHARASTATE.NULL:
                    //VRMとPrefab用
                    globalScale = Vector3.one;
                    gameObject.layer = SystemInfo.layerNo_Default;
                    //リセットして無効化しておく
                    _lookAtVRM.EyeReset();
                    _lookAtVRM.SetEnable(false);
                    isHeadLookAtUpdate = false;
                    isEyeLookAtUpdate = false;
                    break;
                case CHARASTATE.MINIATURE:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    gameObject.layer = SystemInfo.layerNo_GrabObject;
                    break;
                case CHARASTATE.HOLD:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    break;
                case CHARASTATE.ON_CIRCLE:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case CHARASTATE.FIELD:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    gameObject.layer = SystemInfo.layerNo_FieldObject;
                    break;
            }

            //座標の上書き設定
            if (overrideAnchor)
            {
                transform.parent = overrideAnchor;

                //親Scaleの影響を無視する為に算出
                globalScale.x *= 1 / overrideAnchor.lossyScale.x;
                globalScale.y *= 1 / overrideAnchor.lossyScale.y;
                globalScale.z *= 1 / overrideAnchor.lossyScale.z;

                transform.position = overrideAnchor.position;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else transform.parent = null;

            if(charaState == CHARASTATE.MINIATURE || charaState == CHARASTATE.HOLD) transform.localScale = globalScale;
            else transform.localScale = globalScale * CustomScalar;
        }

        // Update is called once per frame
        void Update()
        {
            //座標の上書き
            if (overrideAnchor)
            {
                transform.position = overrideAnchor.position;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            //揺れもの接触振動
            if (charaInfoData && charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                foreach (var springBone in springBoneList)
                {
                    if (springBone.isHit_Any)
                    {
                        if (springBone.isLeft_Any) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, charaInfoData.power, charaInfoData.time);
                        if (springBone.isRight_Any) PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, charaInfoData.power, charaInfoData.time);
                        break;
                    }
                }
            }

            if (isFacialSyncUpdate) _facialSync.MorphUpdate();
        }

        void LateUpdate()
        {
            if (isLipSyncUpdate) _lipSync.MorphUpdate();

            //ポーズ中なら以下処理しない
            if (Time.timeScale == 0) return;
            if (isHeadLookAtUpdate) _headLookAt.HeadUpdate();
            if (isEyeLookAtUpdate) _eyeLookAt.EyeUpdate();
        }

        void OnAnimatorIK()
        {
            if (isHeadLookAtUpdate) _headLookAt.HeadUpdate_OnAnimatorIK();
        }

        public void SetEnabelSpringBones(bool isEnabel)
        {
            foreach (var e in springBoneList)
            {
                e.enabled = isEnabel;
            }
        }

        /// <summary>
        /// アニメーションコントローラーをKeepし、解除する
        /// </summary>
        public void RemoveRunAnime()
        {
            if (keepRunAnime) return;
            keepRunAnime = _animator.runtimeAnimatorController;
            _animator.runtimeAnimatorController = null;
        }

        /// <summary>
        /// 解除したアニメーションコントローラーを元に戻す
        /// </summary>
        public void ReturnRunAnime()
        {
            if (!keepRunAnime) return;
            _animator.runtimeAnimatorController = keepRunAnime;
            keepRunAnime = null;
        }
    }
}