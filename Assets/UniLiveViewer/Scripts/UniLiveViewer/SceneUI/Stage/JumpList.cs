using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    //本当はスクロールビューにしたい、ボコボコボタン生成めちゃ高コスト
    public class JumpList : MonoBehaviour
    {
        public enum TARGET
        {
            NULL,
            CHARA,
            ANIME,
            VMD_LIPSYNC,
            AUDIO,
        }

        public IObservable<(TARGET, int)> OnSelectAsObservable => _selectStream;
        Subject<(TARGET, int)> _selectStream = new();
        TARGET _target = TARGET.NULL;

        [SerializeField] Button_Base Button_BasePrefab;
        [SerializeField] Transform parentAnchor;
        PresetResourceData _presetResourceData;
        ActorEntityManagerService _actorEntityManagerService;
        AnimationAssetManager _animationAssetManager;
        AudioAssetManager _audioAssetManager;
        List<Button_Base> _btnList = new();
        AudioClipSettings _audioClipSettings;

        [Inject]
        public void Construct(
            PresetResourceData presetResourceData,
            ActorEntityManagerService actorEntityManagerService,
            AnimationAssetManager animationAssetManager,
            AudioAssetManager audioAssetManager,
            AudioClipSettings audioClipSettings)
        {
            _presetResourceData = presetResourceData;
            _actorEntityManagerService = actorEntityManagerService;
            _animationAssetManager = animationAssetManager;
            _audioAssetManager = audioAssetManager;
            _audioClipSettings = audioClipSettings;
            Close();
        }

        /// <summary>
        /// 必要に応じてボタンを追加生成
        /// </summary>
        public void BtnInstanceCheck(int needCount)
        {
            const int MAXLINE = 20;//行数
            const float BETWEEN_ROWS = 3.4f;//列間
            const float BETWEEN_LINE = 0.24f;//行間

            if (_btnList.Count < needCount)
            {
                float initX = 0, initY = 0;

                Button_Base btn;
                for (int i = _btnList.Count; i < needCount; i++)
                {
                    initX = i / MAXLINE * BETWEEN_ROWS;
                    initY = 2.0f - (i % MAXLINE * BETWEEN_LINE);

                    btn = Instantiate(Button_BasePrefab);
                    btn.onTrigger += OnClick;
                    btn.transform.parent = parentAnchor;

                    btn.transform.localRotation = Quaternion.identity;

                    //Zファイティング対策
                    if ((initX / 3) % 2 == 0) btn.transform.localPosition = new Vector3(initX, initY, 0);
                    else btn.transform.localPosition = new Vector3(initX, initY, -0.01f);

                    _btnList.Add(btn);
                    btn = null;
                }
            }
        }

        /// <summary>
        /// ボタンにキャラ名を設定する
        /// </summary>
        /// <param name="charaInfoDatas"></param>
        public void SetCharaData(bool isPreset)
        {
            var viewNames = isPreset
                ? _actorEntityManagerService.FbxViewNames : _actorEntityManagerService.VRMViewNames;

            //必要ならボタンを生成
            BtnInstanceCheck(viewNames.Length);

            for (int i = 0; i < _btnList.Count; i++)
            {
                if (i < viewNames.Length)
                {
                    if (viewNames[i] != null) _btnList[i].SetTextMesh(viewNames[i]);
                    else _btnList[i].SetTextMesh(MenuConstants.LoadVRM);

                    if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                }
                else if (viewNames.Length <= i)
                {
                    if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                }
            }
            _target = TARGET.CHARA;
        }

        /// <summary>
        /// ボタンにアニメーション名を設定する
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetAnimeData(bool isPreset)
        {
            var danceInfoData = isPreset
                ? _presetResourceData.DanceInfoData.Select(x => x.ViewName).ToList() : _animationAssetManager.VmdList;

            //必要ならボタンを生成
            BtnInstanceCheck(danceInfoData.Count);

            for (int i = 0; i < _btnList.Count; i++)
            {
                if (i < danceInfoData.Count)
                {
                    _btnList[i].SetTextMesh(danceInfoData[i]);
                    if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                }
                else if (danceInfoData.Count <= i)
                {
                    if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                }
            }
            _target = TARGET.ANIME;
        }

        /// <summary>
        /// ボタンにアニメーション名を設定する
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetLipSyncNames()
        {
            var lipSyncNames = _animationAssetManager.VmdSyncList;

            //必要ならボタンを生成
            BtnInstanceCheck(lipSyncNames.Count);

            for (int i = 0; i < _btnList.Count; i++)
            {
                if (i < lipSyncNames.Count)
                {
                    _btnList[i].SetTextMesh(lipSyncNames[i]);
                    if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                }
                else if (lipSyncNames.Count <= i)
                {
                    if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                }
            }
            _target = TARGET.VMD_LIPSYNC;
        }

        /// <summary>
        /// ボタンにオーディオ名を設定する
        /// </summary>
        public void SetAudioData(bool isPresetAudio)
        {
            if (isPresetAudio)
            {
                //必要ならボタンを生成
                var count = _audioClipSettings.AudioBGM.Count;
                BtnInstanceCheck(count);

                for (int i = 0; i < _btnList.Count; i++)
                {
                    if (i < count)
                    {
                        var name = Path.GetFileName(_audioClipSettings.AudioBGM[i].name);
                        _btnList[i].SetTextMesh(name);
                        if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                //必要ならボタンを生成
                var count = _audioAssetManager.CustomAudios.Count;
                BtnInstanceCheck(count);

                for (int i = 0; i < _btnList.Count; i++)
                {
                    if (i < count)
                    {
                        var name = Path.GetFileName(_audioAssetManager.CustomAudios[i]);
                        _btnList[i].SetTextMesh(name);
                        if (!_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        if (_btnList[i].gameObject.activeSelf) _btnList[i].gameObject.SetActive(false);
                    }
                }
            }
            _target = TARGET.AUDIO;
        }

        /// <summary>
        /// リスト内のいずれかのボタンがクリックされた
        /// </summary>
        /// <param name="btn"></param>
        void OnClick(Button_Base btn)
        {
            //ボタンを特定
            for (int i = 0; i < _btnList.Count; i++)
            {
                if (btn != _btnList[i]) continue;
                _selectStream.OnNext((_target, i));
                Debug.Log($"ジャンプボタンIndex:{i}");
                break;
            }
            gameObject.SetActive(false);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}