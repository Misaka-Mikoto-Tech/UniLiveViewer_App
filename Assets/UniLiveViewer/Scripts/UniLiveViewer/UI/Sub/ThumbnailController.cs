using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using NanaCiel;

namespace UniLiveViewer
{
    public class ThumbnailController : MonoBehaviour
    {
        public event Action OnGenerated;

        Button_Base _btnPrefab;
        Transform parent;
        List<TextMesh> _texts = new List<TextMesh>();
        Button_Base[] _buttons = new Button_Base[15];

        int[] GENERATE_INTERVAL = { 70,210,350 };//ミリ秒
        int[] GENERATE_COUNT = { 1,3,5 };//一括表示数、1～15

        int[] _randomBox;
        string[] _vrmNames;
        CancellationToken _cancellation;

        TextureAssetManager _textureAssetManager;

        public void Initialize(TextureAssetManager textureAssetManager)
        {
            _textureAssetManager = textureAssetManager;
            _btnPrefab = Resources.Load<Button_Base>("Prefabs/Button/btnVRM");
        }

        /// <summary>
        /// サムネ用の空ボタン生成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async UniTask<Button_Base[]> CreateThumbnailButtons()
        {
            _cancellation = this.GetCancellationTokenOnDestroy();
            parent = transform;

            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    index = (i * 5) + j;

                    //生成
                    _buttons[index] = Instantiate(_btnPrefab);
                    _buttons[index].transform.Also((it) =>
                    {
                        it.parent = parent;
                        it.localPosition = new Vector3(-0.3f + (j * 0.15f), 0 - (i * 0.15f));
                        it.localRotation = Quaternion.identity;
                        _texts.Add(it.GetChild(1).GetComponent<TextMesh>());
                    });
                }
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);
            }
            return _buttons;
        }

        /// <summary>
        /// VRMの数だけサムネボタンを生成する
        /// </summary>
        public async UniTask SetThumbnail(string[] vrmNames)
        {
            //一旦全部非表示
            ThumbnailShow(false);
            //全VRMファイル名を取得
            var array = vrmNames;
            //最大15件に丸める
            if (array.Length > 15) _vrmNames = array.Take(15).ToArray();
            else _vrmNames = array;
            //ランダム配列を設定
            _randomBox = new int[_vrmNames.Length];
            for (int i = 0; i < _randomBox.Length; i++) _randomBox[i] = i;
            _randomBox = Shuffle(_randomBox);
            await UniTask.Delay(10, cancellationToken: _cancellation);

            int index = 0;
            int r = UnityEngine.Random.Range(0, 3);
            
            //必要なボタンのみ有効化して設定する
            for (int i = 0; i < _buttons.Length; i++)
            {
                if (i < _vrmNames.Length)
                {
                    //ランダムなボタン順
                    index = _randomBox[i];

                    if (!_buttons[index].gameObject.activeSelf) _buttons[index].gameObject.SetActive(true);
                    //ボタン情報更新
                    _buttons[index].name = _vrmNames[index];
                    _texts[index].text = _vrmNames[index];
                    _texts[index].fontSize = _texts[index].text.FontSizeMatch(500, 25, 40);
                    UpdateSprite(index);

                    if (i % GENERATE_COUNT[r] == 0) OnGenerated?.Invoke();
                    if (i % GENERATE_COUNT[r] == GENERATE_COUNT[r] - 1) await UniTask.Delay(GENERATE_INTERVAL[r], cancellationToken: _cancellation);
                }
            }
        }

        /// <summary>
        /// 表示するサムネを更新
        /// </summary>
        /// <param name="index"></param>
        private void UpdateSprite(int index)
        {
            try
            {
                //サムネイル無しはデフォ画像を流用する仕様
                var spr = _textureAssetManager.Thumbnails[_vrmNames[index]];
                if (spr) _buttons[index].collisionChecker.colorSetting[0].targetSprite.sprite = spr;
            }
            catch
            {
                //Debug.Log("ロジックエラー。アプリを立ち上げ後にキャッシュ画像を削除した？");
                //対策としてボタンを非表示
                if (_texts[index].transform.parent.gameObject.activeSelf)
                {
                    _texts[index].transform.parent.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 一括表示変更
        /// </summary>
        /// <param name="isEnabel"></param>
        private void ThumbnailShow(bool isEnabel)
        {
            for (int i = 0; i < _texts.Count; i++)
            {
                if (_texts[i].transform.parent.gameObject.activeSelf != isEnabel)
                {
                    _texts[i].transform.parent.gameObject.SetActive(isEnabel);
                }
            }
        }

        /// <summary>
        /// ランダムシャッフル（ランダムな2要素を交換→シャッフルされない要素もありえる）
        /// </summary>
        /// <param name="num"></param>
        private int[] Shuffle(int[] inputArray)
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
