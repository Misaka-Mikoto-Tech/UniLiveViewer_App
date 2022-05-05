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
        [SerializeField] private Button_Base btnPrefab;
        private Transform btnParent;
        private List<TextMesh> btnTexts = new List<TextMesh>();
        private Button_Base[] btns = new Button_Base[15];

        public event Action onGenerated;

        private int[] GENERATE_INTERVAL = { 70,210,350 };//ミリ秒
        private int[] GENERATE_COUNT = { 1,3,5 };//一括表示数、1～15

        private int[] randomBox;
        private string[] vrmNames;
        private CancellationToken cancellation_token;

        /// <summary>
        /// サムネ用の空ボタン生成
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async UniTask<Button_Base[]> CreateThumbnailButtons()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();
            btnParent = transform;

            Vector2 btnPos;
            int index = 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    index = (i * 5) + j;

                    //座標調整
                    btnPos.x = -0.3f + (j * 0.15f);
                    btnPos.y = 0 - (i * 0.15f);

                    //生成
                    btns[index] = Instantiate(btnPrefab);

                    //親設定
                    btns[index].transform.parent = btnParent;
                    btns[index].transform.localPosition = new Vector3(btnPos.x, btnPos.y, 0);
                    btns[index].transform.localRotation = Quaternion.identity;

                    //コールバック登録
                    //btns.onTrigger += (b) => LoadVRM(b).Forget();

                    //テキストメッシュをリストに加える
                    btnTexts.Add(btns[index].transform.GetChild(1).GetComponent<TextMesh>());
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
            }

            return btns;
        }

        /// <summary>
        /// VRMの数だけサムネボタンを生成する
        /// </summary>
        public async UniTask SetThumbnail(string[] _vrmNames)
        {
            //一旦全部非表示
            ThumbnailShow(false);

            //全VRMファイル名を取得
            var array = _vrmNames;
            //最大15件に丸める
            if (array.Length > 15) vrmNames = array.Take(15).ToArray();
            else vrmNames = array;
            //ランダム配列を設定
            randomBox = new int[vrmNames.Length];
            for (int i = 0; i < randomBox.Length; i++) randomBox[i] = i;
            randomBox = Shuffle(randomBox);
            await UniTask.Delay(10, cancellationToken: cancellation_token);

            //Button_Base baseButton;
            Sprite spr = null;
            //Texture2D texture = null;
            int index = 0;
            int r = UnityEngine.Random.Range(0, 3);

            //必要なボタンのみ有効化して設定する
            for (int i = 0; i < btns.Length; i++)
            {
                if (i < vrmNames.Length)
                {
                    //ランダムなボタン順
                    index = randomBox[i];

                    if (!btns[index].gameObject.activeSelf) btns[index].gameObject.SetActive(true);
                    //オブジェクト名を変更
                    btns[index].name = vrmNames[index];
                    //ボタンの表示名を変更
                    btnTexts[index].text = vrmNames[index];
                    //文字サイズを調整する
                    btnTexts[index].fontSize = btnTexts[index].text.FontSizeMatch(500, 25, 40);

                    //サムネイルを取得
                    try
                    {
                        spr = FileAccessManager.cacheThumbnails[vrmNames[index]];

                        if (spr)
                        {
                            //サムネの容量で有無を判定
                            //float size = texture.GetRawTextureData().LongLength;
                            //サムネイル無し判定
                            //if (size < 10)
                            //    {
                            //        //defaultのまま
                            //    }
                            //    //サムネイル有り判定
                            //    else
                            //    {
                            //    }

                            //スプライトをセット
                            btns[index].transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
                            //baseButton.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", texture);
                        }
                        else
                        {
                            //defaultのまま
                        }
                    }
                    catch
                    {
                        if (!spr)
                        {
                            //Debug.Log("ロジックエラー。アプリを立ち上げ後にキャッシュ画像を削除した？");
                            //対策としてボタンを非表示
                            if (btnTexts[index].transform.parent.gameObject.activeSelf)
                            {
                                btnTexts[index].transform.parent.gameObject.SetActive(false);
                            }
                        }
                    }
                    finally
                    {
                        spr = null;
                    }

                    if (i % GENERATE_COUNT[r] == 0)
                    {
                        onGenerated?.Invoke();
                        await UniTask.Delay(GENERATE_INTERVAL[r], cancellationToken: cancellation_token);
                    }
                }
            }
        }

        private void ThumbnailShow(bool isEnabel)
        {
            for (int i = 0; i < btnTexts.Count; i++)
            {
                if (btnTexts[i].transform.parent.gameObject.activeSelf != isEnabel)
                {
                    btnTexts[i].transform.parent.gameObject.SetActive(isEnabel);
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
