using UniLiveViewer.Menu;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.Expression
{
    public class ExpressionService
    {
        bool _canLipSync = true;
        bool _canFacialSync = true;
        CurrentMode _animationMode = CurrentMode.PRESET;

        readonly IActorService _actorService;
        readonly ILipSync _lipSync;
        readonly IFacialSync _facialSync;

        [Inject]
        public ExpressionService(
            IActorService actorService,
            ILipSync lipSync,
            IFacialSync facialSync)
        {
            _actorService = actorService;
            _lipSync = lipSync;
            _facialSync = facialSync;
        }

        /// <summary>
        /// VMD再生走ってから呼ぶこと
        /// </summary>
        /// <param name="mode"></param>
        public void OnChangeMode(CurrentMode mode)
        {
            _animationMode = mode;
            OnChangeFacialSync(_canFacialSync);
            OnChangeLipSync(_canLipSync);
        }

        public void OnChangeFacialSync(bool isEnable)
        {
            _canFacialSync = isEnable;

            if (_animationMode == CurrentMode.CUSTOM)
            {
                var vmdPlayer = _actorService.ActorEntity().Value.GetVMDPlayer;
                if (vmdPlayer.morphPlayer_vrm == null) return;
                vmdPlayer.morphPlayer_vrm.isUpdateFace = isEnable;
            }
            _facialSync.MorphReset();
        }

        public void OnChangeLipSync(bool isEnable)
        {
            _canLipSync = isEnable;

            if (_animationMode == CurrentMode.CUSTOM)
            {
                var vmdPlayer = _actorService.ActorEntity().Value.GetVMDPlayer;
                if (vmdPlayer.morphPlayer_vrm == null) return;
                vmdPlayer.morphPlayer_vrm.isUpdateMouth = isEnable;
            }
            _lipSync.MorphReset();
        }

        public void OnLateTick()
        {
            if (Time.timeScale == 0) return; //ポーズ中なら以下処理しない
            if (_animationMode != CurrentMode.PRESET) return;

            if (_canFacialSync) _facialSync.MorphUpdate();
            if (_canLipSync) _lipSync.MorphUpdate();
        }
    }
}
