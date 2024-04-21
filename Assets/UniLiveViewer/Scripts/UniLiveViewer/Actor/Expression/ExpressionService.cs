﻿using UniLiveViewer.Menu;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.Expression
{
    public class ExpressionService
    {
        bool _canLipSync = true;
        bool _canFacialSync = true;
        CurrentMode _animationMode = CurrentMode.PRESET;

        readonly IActorEntity _actorEntity;
        readonly ILipSync _lipSync;
        readonly IFacialSync _facialSync;

        [Inject]
        public ExpressionService(
            IActorEntity actorEntity,
            ILipSync lipSync,
            IFacialSync facialSync)
        {
            _actorEntity = actorEntity;
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
                if (_actorEntity.ActorEntity().Value == null) return;
                var vmdPlayer = _actorEntity.ActorEntity().Value.GetVMDPlayer;
                if (vmdPlayer.MorphPlayerVRM == null) return;
                vmdPlayer.MorphPlayerVRM.SetFaceUpdate(isEnable);
            }
            _facialSync.MorphReset();
        }

        public void OnChangeLipSync(bool isEnable)
        {
            _canLipSync = isEnable;

            if (_animationMode == CurrentMode.CUSTOM)
            {
                if (_actorEntity.ActorEntity().Value == null) return;
                var vmdPlayer = _actorEntity.ActorEntity().Value.GetVMDPlayer;
                if (vmdPlayer.MorphPlayerVRM == null) return;
                vmdPlayer.MorphPlayerVRM.SetLipUpdate(isEnable);
            }
            _lipSync.MorphReset();
        }

        public void MorphReset()
        {
            _facialSync.MorphReset();
            _lipSync.MorphReset();
        }

        public void OnLateTick()
        {
            if (Time.timeScale == 0) return; //ポーズ中なら以下処理しない
            if (_animationMode != CurrentMode.PRESET) return;
            if (_canFacialSync) _facialSync.Morph();
            if (_canLipSync) _lipSync.Morph();
        }
    }
}
