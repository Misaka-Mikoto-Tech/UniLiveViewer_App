﻿using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Collections.Generic;
using System.Threading;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class ThumbnailService
    {
        public IObservable<Button_Base> OnClickAsObservable => _clickStream;
        readonly Subject<Button_Base> _clickStream = new();

        Button_Base _btnPrefab;
        readonly List<TextMesh> _texts = new();
        readonly Button_Base[] _buttons = new Button_Base[20];

        int[] GENERATE_INTERVAL = { 70, 210, 350 };//ミリ秒
        int[] GENERATE_COUNT = { 1, 3, 5 };//一括表示数、1～15

        int[] _randomBox;

        readonly AudioSourceService _audioSourceService;
        readonly ThumbnailAnchor _thumbnailAnchor;
        readonly TextureAssetManager _textureAssetManager;
        readonly ActorEntityManagerService _actorEntityManager;

        [Inject]
        public ThumbnailService(
            AudioSourceService audioSourceService,
            ThumbnailAnchor thumbnailAnchor,
            TextureAssetManager textureAssetManager,
            ActorEntityManagerService actorEntityManager)
        {
            _audioSourceService = audioSourceService;
            _thumbnailAnchor = thumbnailAnchor;
            _textureAssetManager = textureAssetManager;
            _actorEntityManager = actorEntityManager;
        }

        public async UniTask InitializeAsync(CancellationToken cancellation)
        {
            _btnPrefab = Resources.Load<Button_Base>("Prefabs/Button/btnVRM");
            CreateButtonAsync(cancellation).Forget();
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// サムネ用の空ボタン生成
        /// </summary>
        async UniTask<Button_Base[]> CreateButtonAsync(CancellationToken cancellation)
        {
            var index = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    index = (i * 5) + j;

                    //生成
                    _buttons[index] = GameObject.Instantiate<Button_Base>(_btnPrefab);
                    _buttons[index].transform.Also((it) =>
                    {
                        it.parent = _thumbnailAnchor.transform;
                        it.localPosition = new Vector3(-0.2f + (j * 0.12f), 0 - (i * 0.12f));
                        it.localRotation = Quaternion.identity;
                        _texts.Add(it.GetChild(1).GetComponent<TextMesh>());
                    });

                    // TODO: 整理する
                    _buttons[index].onTrigger += (b) =>
                    {
                        //重複クリックできないようにボタンを無効化
                        SetEnableRoot(false);
                        _audioSourceService.PlayOneShot(0);
                        _actorEntityManager.RegisterVRM(new VRMLoadData(b.name));
                        _clickStream.OnNext(b);
                    };
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
            }
            return _buttons;
        }

        /// <summary>
        /// VRM分のサムネボタンを設定
        /// </summary>
        public async UniTask BeginAsync(CancellationToken cancellation)
        {
            SetEnableRoot(true);

            //一旦全部非表示
            ThumbnailShow(false);
            var clampedData = _textureAssetManager.CurrentVRMNamesDatas.ClampedData;
            //ランダム配列を設定
            _randomBox = new int[clampedData.Length];
            for (int i = 0; i < _randomBox.Length; i++) _randomBox[i] = i;
            _randomBox = Shuffle(_randomBox);
            await UniTask.Delay(10, cancellationToken: cancellation);

            var index = 0;
            var random = UnityEngine.Random.Range(0, 3);

            //必要なボタンのみ有効化して設定する
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (i < clampedData.Length)
                {
                    //ランダムなボタン順
                    index = _randomBox[i];

                    if (!_buttons[index].gameObject.activeSelf) _buttons[index].gameObject.SetActive(true);
                    //ボタン情報更新
                    _buttons[index].name = clampedData[index];
                    _texts[index].text = clampedData[index];
                    _texts[index].fontSize = _texts[index].text.FontSizeMatch(500, 25, 40);
                    UpdateSprite(clampedData, index);

                    if (i % GENERATE_COUNT[random] == 0)
                    {
                        UniTask.Delay(500, cancellationToken: cancellation)
                            .ContinueWith(() => _audioSourceService.PlayOneShot(1)).Forget();
                    }
                    if (i % GENERATE_COUNT[random] == GENERATE_COUNT[random] - 1) await UniTask.Delay(GENERATE_INTERVAL[random], cancellationToken: cancellation);
                }
            }
        }

        void SetEnableRoot(bool isEnabel)
        {
            if (_thumbnailAnchor.gameObject.activeSelf == isEnabel) return;
            _thumbnailAnchor.gameObject.SetActive(isEnabel);
        }

        /// <summary>
        /// 表示するサムネを更新
        /// </summary>
        void UpdateSprite(string[] clampedData, int index)
        {
            try
            {
                //サムネイル無しはデフォ画像を流用する仕様
                var spr = _textureAssetManager.Thumbnails[clampedData[index]];
                if (spr) _buttons[index].collisionChecker.colorSetting[0].targetSprite.sprite = spr;
            }
            catch
            {
                //Debug.Log("ロジックエラー。アプリを立ち上げ後にキャッシュ画像を削除した？");
                //対策としてボタンを非表示
                var go = _texts[index].transform.parent.gameObject;
                if (!go.activeSelf) return;
                go.SetActive(false);
            }
        }

        /// <summary>
        /// 一括表示変更
        /// </summary>
        void ThumbnailShow(bool isEnabel)
        {
            for (int i = 0; i < _texts.Count; i++)
            {
                if (_texts[i].transform.parent.gameObject.activeSelf == isEnabel) continue;
                _texts[i].transform.parent.gameObject.SetActive(isEnabel);
            }
        }

        /// <summary>
        /// ランダムシャッフル（ランダムな2要素を交換→シャッフルされない要素もありえる）
        /// </summary>
        int[] Shuffle(int[] inputArray)
        {
            for (int i = 0; i < inputArray.Length; i++)
            {
                int temp = inputArray[i];
                int randomIndex = UnityEngine.Random.Range(0, inputArray.Length);
                inputArray[i] = inputArray[randomIndex];
                inputArray[randomIndex] = temp;
            }
            return inputArray;
        }
    }
}
