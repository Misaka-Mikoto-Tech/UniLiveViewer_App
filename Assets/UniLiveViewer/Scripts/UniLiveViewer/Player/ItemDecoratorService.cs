using System;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.OVRCustom;
using UniLiveViewer.Player.HandMenu;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// Playerの持っているアイテムをActorに装飾する
    /// TODO: OVRGrabberをLS管理にしないと本当のサービスにならない
    /// </summary>
    public class ItemDecoratorService
    {
        const int PIECE_ANGLE = 45;

        Vector2 _input;
        bool _ismaterialSelection;
        int _currentIndex = 0;

        readonly OVRGrabber_UniLiveViewer _hand;
        readonly RootAudioSourceService _audioSourceService;
        readonly ItemMaterialSelectionService _itemMaterialSelection;
        readonly PlayableAnimationClipService _playableAnimationClipService;
        readonly PlayableDirector _playableDirector;

        public ItemDecoratorService(
            OVRGrabber_UniLiveViewer hand,
            RootAudioSourceService audioSourceService,
            ItemMaterialSelectionService itemMaterialSelection,
            PlayableAnimationClipService playableAnimationClipService,
            PlayableDirector playableDirector)
        {
            _hand = hand;
            _audioSourceService = audioSourceService;
            _itemMaterialSelection = itemMaterialSelection;
            _playableAnimationClipService = playableAnimationClipService;
            _playableDirector = playableDirector;
        }

        public void OnChangeActionButton()
        {
            //テクスチャ変更UIを表示・非表示
            _ismaterialSelection = !_ismaterialSelection;

            if (_ismaterialSelection)
            {
                //TODO: 見直す
                if (_hand.GrabbedObj.Value == null) return;
                if (!_hand.GrabbedObj.Value.TryGetComponent<DecorationItemInfo>(out var itemInfo)) return;
                _itemMaterialSelection.ChangeShow((int)_hand.HandType, _ismaterialSelection, itemInfo);
            }
            else
            {
                _itemMaterialSelection.ChangeShow((int)_hand.HandType, _ismaterialSelection, null);
            }
        }

        public void CloseMenu()
        {
            _ismaterialSelection = false;
            _itemMaterialSelection.ChangeShow((int)_hand.HandType, false, null);
        }

        /// <summary>
        /// アタッチ時はメニュー開閉音の代わりにアタッチ結果の成功失敗音にしたいため
        /// </summary>
        public void OnClickTriggerButton()
        {
            _ismaterialSelection = false;
            _itemMaterialSelection.ForceCloseMenu((int)_hand.HandType);

            if (TryAttachmentItem(_hand))
            {
                _audioSourceService.PlayOneShot(AudioSE.AttachSuccess);
            }
            else
            {
                _audioSourceService.PlayOneShot(AudioSE.ObjectDelete);
            }
        }

        public void OnChangeStickInput(Vector2 v)
        {
            _input = v;
        }

        public void OnUpdate()
        {
            if (!_ismaterialSelection) return;
            if (_input.sqrMagnitude <= 0.25f) return;

            var rad = Mathf.Atan2(_input.x, _input.y);
            var degree = rad * Mathf.Rad2Deg;
            if (degree < 0 - (PIECE_ANGLE / 2)) degree += 360;
            var currentIndex = (int)Math.Round(degree / PIECE_ANGLE);//Mathfは四捨五入ではない→.NET使用

            if (_currentIndex != currentIndex)
            {
                _currentIndex = currentIndex;
                _itemMaterialSelection.SetItemTexture((int)_hand.HandType, currentIndex);
            }
        }

        /// <summary>
        /// TODO:無駄に複雑なので仕様整理したい
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        bool TryAttachmentItem(OVRGrabber_UniLiveViewer hand)
        {
            var grabObj = hand.GrabbedObj.Value;
            if (grabObj == null || !grabObj.IsBothHandsGrab)
            {
                return false;
            }

            //結果によらず先に離しちゃう
            hand.FoeceGrabEnd();

            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual)
            {
                GameObject.Destroy(grabObj.gameObject);
                return false;
            }

            if (grabObj.HitCollider == null || !grabObj.HitCollider.TryGetComponent<AttachPoint>(out var ap))
            {
                GameObject.Destroy(grabObj.gameObject);
                return false;
            }

            if (!grabObj.TryGetComponent<DecorationItemInfo>(out var decoration))
            {
                GameObject.Destroy(grabObj.gameObject);
                return false;
            }

            if (!decoration.TryAttachment())
            {
                GameObject.Destroy(grabObj.gameObject);
                return false;
            }

            //手の時だけ特殊
            if (ap.HumanBodyBones == HumanBodyBones.LeftHand ||
                ap.HumanBodyBones == HumanBodyBones.RightHand)
            {
                _playableAnimationClipService.SetHandAnimation(ap.InstanceId, ap.HumanBodyBones, true);
            }
            return true;
        }
    }
}