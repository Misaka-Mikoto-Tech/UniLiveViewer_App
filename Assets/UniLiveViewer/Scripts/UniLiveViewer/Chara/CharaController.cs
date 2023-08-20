using System.Collections.Generic;
using UnityEngine;
using VRM;
using NanaCiel;

namespace UniLiveViewer
{
    public class CharaController : MonoBehaviour
    {
        public CharaEnums.STATE GetCharaState => _charaState;
        [SerializeField] CharaEnums.STATE _charaState;
        public CharaEnums.ANIMATION_MODE AnimationMode;
        public Animator GetAnimator => _animator;
        Animator _animator;

        /// <summary>
        /// 初期化はAwakeでもNG、Prefab用コピーしてるのが良くない
        /// </summary>
        public List<VRMSpringBone> SpringBoneList = new List<VRMSpringBone>();

        //Sync
        public bool CanLipSync;
        public bool CanFacialSync;
        public ILipSync LipSync;
        public IFacialSync FacialSync;

        //LookAt
        public LookAtBase LookAt;
        bool _canHeadLookAt;
        bool _canEyeLookAt;
        public IHeadLookAt HeadLookAt;
        public IEyeLookAt EyeLookAt;
        public ILookAtVRM LookAtVRM;

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
        public string BindTrackName;
        [SerializeField] RuntimeAnimatorController _cachedAnimatorController;//アニメーション無効化の際に直前状態をキープ
        public AnimationClip CachedClip_handL;//手アニメーション(握りから戻す際に必要)
        public AnimationClip CachedClip_handR;//手アニメーション(握りから戻す際に必要)
        [SerializeField] Transform _overrideAnchor;//座標の上書き用
        
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
                LipSync = transform.GetComponentInChildren<ILipSync>();
                FacialSync = transform.GetComponentInChildren<IFacialSync>();
                LookAt = transform.GetComponent<LookAtBase>();
                HeadLookAt = transform.GetComponent<IHeadLookAt>();
                EyeLookAt = transform.GetComponent<IEyeLookAt>();
            }


            _charaState = CharaEnums.STATE.NULL;
            AnimationMode = CharaEnums.ANIMATION_MODE.CLIP;
            CanLipSync = false;
            CanFacialSync = false;
            _canHeadLookAt = false;
            _canEyeLookAt = false;

            _customScalar = StageSettingService.UserProfile.InitCharaSize;
        }

        public void SetLookAt(bool isEnable)
        {
            _canHeadLookAt = isEnable;
            _canEyeLookAt = isEnable;
        }

        public void InitVRMSync(LipSync_VRM lipSync, FacialSync_VRM facialSync, VRMBlendShapeProxy blendShapeProxy)
        {
            lipSync.transform.name = "LipSyncController";
            lipSync.transform.parent = transform;
            lipSync.vrmBlendShape = blendShapeProxy;
            LipSync = lipSync;

            facialSync.transform.name = "FaceSyncController";
            facialSync.transform.parent = transform;
            facialSync.vrmBlendShape = blendShapeProxy;
            FacialSync = facialSync;
        }

        public void InitLookAtController()
        {
            if (charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                gameObject.AddComponent<NormalizedBoneGenerator>();

                //VRMボーン
                if (GetComponent<VRMLookAtBoneApplyer_Custom>() != null)
                {
                    LookAt =  gameObject.AddComponent<LookAt_VRMBone>().GetComponent<LookAtBase>();
                    HeadLookAt = transform.GetComponent<IHeadLookAt>();
                    EyeLookAt = transform.GetComponent<IEyeLookAt>();
                    LookAtVRM = transform.GetComponent<ILookAtVRM>();

                    charaInfoData.charaType = CharaInfoData.CHARATYPE.VRM_Bone;
                    ((LookAt_VRMBone)LookAtVRM)._vrmEyeApplyer = GetComponent<VRMLookAtBoneApplyer_Custom>();

                    var lookAtHead = transform.GetComponent<VRMLookAtHead_Custom>();
                    lookAtHead.Target = LookAtVRM.GetLookAtTarget();
                    lookAtHead.UpdateType = UpdateType.LateUpdate;
                }
                //VRMブレンドシェイプ
                else if (GetComponent<VRMLookAtBlendShapeApplyer_Custom>() != null)
                {
                    LookAt = gameObject.AddComponent<LookAt_VRMBone>().GetComponent<LookAtBase>();
                    HeadLookAt = transform.GetComponent<IHeadLookAt>();
                    EyeLookAt = transform.GetComponent<IEyeLookAt>();
                    LookAtVRM = transform.GetComponent<ILookAtVRM>();

                    charaInfoData.charaType = CharaInfoData.CHARATYPE.VRM_BlendShape;
                    ((LookAt_VRMBlendShape)LookAtVRM)._vrmEyeApplyer = GetComponent<VRMLookAtBlendShapeApplyer_Custom>();

                    var lookAtHead = transform.GetComponent<VRMLookAtHead_Custom>();
                    lookAtHead.Target = LookAtVRM.GetLookAtTarget();
                    lookAtHead.UpdateType = UpdateType.LateUpdate;
                }
                //UVはそもそも用意されて無いのだ...？
            }
        }

        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="setState"></param>
        /// <param name="overrideTarget">対象位置に座標を合わせる</param>
        public void SetState(CharaEnums.STATE setState, Transform overrideTarget)
        {
            Vector3 globalScale = Vector3.zero;

            _overrideAnchor = overrideTarget;
            _charaState = setState;
            switch (_charaState)
            {
                case CharaEnums.STATE.NULL:
                    //VRMとPrefab用
                    globalScale = Vector3.one;
                    gameObject.layer = Constants.LayerNoDefault;
                    //リセットして無効化しておく
                    LookAtVRM.EyeReset();
                    LookAtVRM.SetEnable(false);
                    SetLookAt(false);
                    break;
                case CharaEnums.STATE.MINIATURE:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    gameObject.layer = Constants.LayerNoGrabObject;
                    break;
                case CharaEnums.STATE.HOLD:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    break;
                case CharaEnums.STATE.ON_CIRCLE:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case CharaEnums.STATE.FIELD:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    gameObject.layer = Constants.LayerNoFieldObject;
                    break;
            }

            //座標の上書き設定
            if (_overrideAnchor)
            {
                transform.parent = _overrideAnchor;

                //親Scaleの影響を無視する為に算出
                globalScale.x *= 1 / _overrideAnchor.lossyScale.x;
                globalScale.y *= 1 / _overrideAnchor.lossyScale.y;
                globalScale.z *= 1 / _overrideAnchor.lossyScale.z;

                transform.position = _overrideAnchor.position;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else transform.parent = null;

            if(_charaState == CharaEnums.STATE.MINIATURE || _charaState == CharaEnums.STATE.HOLD) transform.localScale = globalScale;
            else transform.localScale = globalScale * CustomScalar;

            Debug.LogWarning(transform.position);
        }

        // Update is called once per frame
        void Update()
        {
            //座標の上書き
            if (_overrideAnchor)
            {
                transform.position = _overrideAnchor.position;
                transform.localRotation = Quaternion.Euler(Vector3.zero);
            }

            //揺れもの接触振動
            if (charaInfoData && charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                foreach (var springBone in SpringBoneList)
                {
                    if (springBone.isHit_Any)
                    {
                        if (springBone.isLeft_Any) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, charaInfoData.power, charaInfoData.time);
                        if (springBone.isRight_Any) ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, charaInfoData.power, charaInfoData.time);
                        break;
                    }
                }
            }

            if (CanFacialSync) FacialSync.MorphUpdate();
        }

        void LateUpdate()
        {
            if (CanLipSync) LipSync.MorphUpdate();

            //ポーズ中なら以下処理しない
            if (Time.timeScale == 0) return;
            if (_canHeadLookAt) HeadLookAt.HeadUpdate();
            if (_canEyeLookAt) EyeLookAt.EyeUpdate();
        }

        void OnAnimatorIK()
        {
            if (_canHeadLookAt) HeadLookAt.HeadUpdate_OnAnimatorIK();
        }

        public void SetEnabelSpringBones(bool isEnabel)
        {
            foreach (var e in SpringBoneList)
            {
                e.enabled = isEnabel;
            }
        }

        /// <summary>
        /// アニメーションコントローラーをKeepし、解除する
        /// </summary>
        public void RemoveRunAnime()
        {
            if (_cachedAnimatorController) return;
            _cachedAnimatorController = _animator.runtimeAnimatorController;
            _animator.runtimeAnimatorController = null;
        }

        /// <summary>
        /// 解除したアニメーションコントローラーを元に戻す
        /// </summary>
        public void ReturnRunAnime()
        {
            if (!_cachedAnimatorController) return;
            _animator.runtimeAnimatorController = _cachedAnimatorController;
            _cachedAnimatorController = null;
        }
    }
}