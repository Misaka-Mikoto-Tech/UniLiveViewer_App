using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.Menu;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityVMDReader;
using VContainer;

namespace UniLiveViewer.Actor.Animation
{
    public class AnimationService
    {
        /// <summary>
        /// 停止時Motionが死ぬのでTimelineがManual時のみ脱ぐ
        /// ここにダンスフォーマットは関係しない
        /// </summary>
        RuntimeAnimatorController _cachedAnimatorController;
        ActorEntity _actorEntity;
        CurrentMode _currentMode;

        readonly PlayableDirector _playableDirector;
        readonly PlayableAnimationClipService _playableAnimationClipService;
        readonly PresetResourceData _presetResourceData;
        readonly VMDData _vmdData;

        [Inject]
        public AnimationService(
            PlayableDirector playableDirector,
            PlayableAnimationClipService playableAnimationClipService,
            RuntimeAnimatorController cachedAnimatorController,
            PresetResourceData presetResourceData,
            VMDData vmdData)
        {
            _playableDirector = playableDirector;
            _playableAnimationClipService = playableAnimationClipService;
            _cachedAnimatorController = cachedAnimatorController;
            _presetResourceData = presetResourceData;
            _vmdData = vmdData;
        }

        public void OnChangeAnimator(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
            if (_actorEntity == null) return;

            var animator = _actorEntity.GetAnimator;
            animator.runtimeAnimatorController = _cachedAnimatorController;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            //animator.applyRootMotion = false;最早関係なさそう

            //自分で検知する
            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;
            RemoveRuntimeAnimatorController();
        }

        public async UniTask SetAnimationAsync(CurrentMode mode, int index, bool isReverse, CancellationToken cancellation)
        {
            if (_actorEntity == null) return;

            _currentMode = mode;
            if (_currentMode == CurrentMode.PRESET)
            {
                _actorEntity.GetVMDPlayer.ClearBaseAndSyncData();
                _actorEntity.GetAnimator.enabled = true;

                // ポータル上のアニメーションバインド
                var data = _presetResourceData.DanceInfoData[index];
                data.isReverse = isReverse;
                _playableAnimationClipService.BindingNewClips(data);
            }
            else if (_currentMode == CurrentMode.CUSTOM)
            {
                _actorEntity.GetAnimator.enabled = false;
                var vmd = _vmdData.TryGetCurrentVMD();
                var folderPath = PathsInfo.GetFullPath(FolderType.MOTION) + "/";
                var fileName = _vmdData.GetCurrentName();
                await PlayVMDAsync(vmd, folderPath, fileName, true, cancellation);
                await TrySetSyncVMDAsync(cancellation);

                // 空データで実質nullバインドする
                var data = _presetResourceData.VMDDanceInfoData;
                _playableAnimationClipService.BindingNewClips(data);
            }
        }

        /// <summary>
        /// VMDを再生する
        /// TODO: いつかちゃんとする
        /// </summary>
        /// <param name="vmd">nullなら新規扱い</param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="isBaseMotion">通常か表情か</param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        async UniTask PlayVMDAsync(VMD vmd, string folderPath, string fileName, bool isBaseMotion, CancellationToken cancellation)
        {
            //新規
            if (vmd == null)
            {
                var isSmoothVMD = true;
                var boneAmplifier = FileReadAndWriteUtility.UserProfile.VMDScale;
                var newVMD = await _actorEntity.GetVMDPlayer.Starter(
                    null, folderPath, fileName, boneAmplifier, isSmoothVMD, isBaseMotion, cancellation);
                _vmdData.Add(fileName, newVMD);
            }
            //既存は使いまわす
            else
            {
                var isSmoothVMD = true;
                var boneAmplifier = FileReadAndWriteUtility.UserProfile.VMDScale;
                await _actorEntity.GetVMDPlayer.Starter(
                    vmd, folderPath, fileName, boneAmplifier, isSmoothVMD, isBaseMotion, cancellation);
            }
        }

        async UniTask TrySetSyncVMDAsync(CancellationToken cancellation)
        {
            if (_currentMode != CurrentMode.CUSTOM) return;
            if (!_vmdData.TryGetCurrentSyncName(out string syncFileName)) return;

            if (syncFileName == TimelineConstants.LIPSYNC_NONAME)
            {
                _actorEntity.GetVMDPlayer.ClearSyncData();
            }
            else
            {
                var vmd = _vmdData.TryGetCurrentSyncVMD(syncFileName);
                var folderPath = PathsInfo.GetFullPath_LipSync() + "/";
                await PlayVMDAsync(vmd, folderPath, syncFileName, false, cancellation);
            }
        }

        public void OnLateTick()
        {
            if (_actorEntity == null) return;
            _actorEntity.GetVMDPlayer.ToeIKReset();
        }

        /// <summary>
        /// アニメーションコントローラーをKeepし、解除する
        /// </summary>
        public void RemoveRuntimeAnimatorController()
        {
            if (_actorEntity == null) return;
            if (!_actorEntity.GetAnimator.runtimeAnimatorController) return;
            _cachedAnimatorController = _actorEntity.GetAnimator.runtimeAnimatorController;
            _actorEntity.GetAnimator.runtimeAnimatorController = null;
        }

        /// <summary>
        /// 解除したアニメーションコントローラーを元に戻す
        /// </summary>
        public void ReturnRuntimeAnimatorController()
        {
            if (_actorEntity == null) return;
            if (!_cachedAnimatorController) return;
            _actorEntity.GetAnimator.runtimeAnimatorController = _cachedAnimatorController;
            _cachedAnimatorController = null;
        }
    }
}
