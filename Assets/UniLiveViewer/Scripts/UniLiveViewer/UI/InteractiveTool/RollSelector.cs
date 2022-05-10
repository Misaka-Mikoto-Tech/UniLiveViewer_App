using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    //作ってみたが廃止しそう
    public class RollSelector : MonoBehaviour
    {
        [Header("＜UI＞")]
        [SerializeField] private TouchCollision[] touchCol = new TouchCollision[2];
        [SerializeField] private TextMesh[] textMesh = new TextMesh[5];

        public int Current { get; private set; }
        private List<string> baseList = new List<string>();
        private List<int> index = new List<int>();
        private Animator anime;
        private int stateDefault;
        private CancellationTokenSource cts;

        public event Action onTouch;

        void Awake()
        {
            anime = GetComponent<Animator>(); 
        }

        private void Start()
        {
            stateDefault = Animator.StringToHash("Default");
            touchCol[0].onHit += (name) => RollUpdate(name).Forget();
            touchCol[1].onHit += (name) => RollUpdate(name).Forget();
        }

        private void OnEnable()
        {
            cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            cts.Cancel();
        }

        public string GetCurrentMatName()
        {
            return baseList[Current];
        }

        public void Init(List<string> _list)
        {
            //前回データを削除
            Current = 0;
            if (index.Count > 0) index.Clear();
            if (baseList.Count > 0) baseList.Clear();

            //コピー
            baseList = _list;

            //初期のリストアップする分を設定
            index.Add(baseList.Count - 2 - Current);
            index.Add(baseList.Count - 1 - Current);
            index.Add(Current);
            index.Add(Current + 1);
            index.Add(Current + 2);

            foreach (var e in index) 
            {
                Debug.Log("index:" + e);
            }

            //テキストに反映
            TextUpdate(" (Instance)", "");
        }

        private async UniTask RollUpdate(TouchCollision target)
        {
            bool isUpRoll = target.name.Contains("Up");
            target._collider.enabled = false;

            //触れたコライダー側に回転アニメーション
            if (isUpRoll)
            {
                Current--;
                if (Current < 0) Current = baseList.Count - 1;
                anime.SetBool("Roll_Up", true);
            }
            else
            {
                Current++;
                if (Current >= baseList.Count) Current = 0;
                anime.SetBool("Roll_Down", true);
            }

            //イベント
            onTouch?.Invoke();
            await UniTask.Delay(50, cancellationToken: cts.Token);

            if (isUpRoll)
            {
                anime.SetBool("Roll_Up", false);
                index.RemoveAt(index.Count - 1);//末尾を削除

                if (Current - 2 < 0) index.Insert(0, baseList.Count - (2 - Current));//先頭に追加
                else index.Insert(0, Current - 2);//先頭に追加
            }
            else
            {
                anime.SetBool("Roll_Down", false);
                index.RemoveAt(0);//先頭を削除

                if (Current + 2 >= baseList.Count) index.Add(Current + 2 - baseList.Count);//末尾に追加
                else index.Add(Current + 2);//末尾に追加
            }

            //AnimatorStateが確実に切り替わるまで少し待つ
            await UniTask.Delay(50, cancellationToken: cts.Token);

            while (anime.GetCurrentAnimatorStateInfo(0).shortNameHash != stateDefault)
            {
                await UniTask.Delay(10, cancellationToken: cts.Token);
            }

            //テキストに反映
            TextUpdate(" (Instance)", "");

            target._collider.enabled = true;
        }

        /// <summary>
        /// 表示更新
        /// </summary>
        /// <param name="oldChar">Replaceの機能</param>
        /// <param name="newChar">Replaceの機能</param>
        private void TextUpdate(string oldChar, string newChar)
        {
            string sReplaceName;
            for (int i = 0;i< textMesh.Length;i++)
            {
                if(i < baseList.Count)
                {
                    sReplaceName = baseList[index[i]].Replace(oldChar, newChar);
                    textMesh[i].text = $"{index[i]}: {sReplaceName}";
                }
                else
                {
                    textMesh[i].text = "";
                }
            }
        }
    }
}