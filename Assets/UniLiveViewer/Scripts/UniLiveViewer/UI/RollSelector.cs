using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    //作ってみたが廃止しそう
    public class RollSelector : MonoBehaviour
    {
        [SerializeField] private TouchCollision[] touchCol = new TouchCollision[2];
        [SerializeField] private TextMesh[] textMesh = new TextMesh[5];
        public int current { get; private set; }
        private Animator anime;
        public List<string> baseList = new List<string>();
        public List<int> listUp = new List<int>();

        public event Action onTouch;

        void Awake()
        {
            anime = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            StartCoroutine(RollUpdate());
        }

        private void OnDisable()
        {
            StopCoroutine(RollUpdate());
        }

        public void init(List<string> _list)
        {
            //前回データを削除
            current = 0;
            if (listUp.Count > 0) listUp.Clear();
            if (baseList.Count > 0) baseList.Clear();

            //コピー
            baseList = _list;

            //初期のリストアップする分を設定
            listUp.Add(baseList.Count - 2 - current);
            listUp.Add(baseList.Count - 1 - current);
            listUp.Add(current);
            listUp.Add(current + 1);
            listUp.Add(current + 2);

            //テキストに反映
            TextUpdate(" (Instance)", "");
        }

        private IEnumerator RollUpdate()
        {
            int stateDefault = Animator.StringToHash("Default");

            //ページが開かれている間
            while (transform)
            {
                //触れたコライダー側に回転アニメーション
                if (touchCol[0].isTouch)
                {
                    current--;
                    if (current < 0) current = baseList.Count - 1;

                    //イベント
                    if (onTouch != null) onTouch();

                    anime.SetBool("Roll_Up", true);

                    yield return null;
                    anime.SetBool("Roll_Up", false);

                    listUp.RemoveAt(listUp.Count - 1);//末尾を削除

                    if (current - 2 < 0)
                    {
                        listUp.Insert(0, baseList.Count - (2 - current));//先頭に追加
                    }
                    else
                    {
                        listUp.Insert(0, current - 2);//先頭に追加
                    }

                    yield return new WaitForSeconds(0.01f);//AnimatorStateが確実に切り替わるまで少し待つ
                    while (anime.GetCurrentAnimatorStateInfo(0).shortNameHash != stateDefault)
                    {
                        yield return null;
                    }
                    //テキストに反映
                    TextUpdate(" (Instance)", "");

                    touchCol[0].isTouch = false;
                }
                else if (touchCol[1].isTouch)
                {
                    current++;
                    if (current >= baseList.Count) current = 0;

                    //イベント
                    if (onTouch != null) onTouch();

                    anime.SetBool("Roll_Down", true);

                    yield return null;
                    anime.SetBool("Roll_Down", false);

                    listUp.RemoveAt(0);//先頭を削除
                    if (current + 2 >= baseList.Count)
                    {
                        listUp.Add(current + 2 - baseList.Count);//末尾に追加
                    }
                    else
                    {
                        listUp.Add(current + 2);//末尾に追加
                    }

                    yield return new WaitForSeconds(0.01f);//AnimatorStateが確実に切り替わるまで少し待つ
                    while (anime.GetCurrentAnimatorStateInfo(0).shortNameHash != stateDefault)
                    {
                        yield return null;
                    }

                    //テキストに反映
                    TextUpdate(" (Instance)", "");

                    touchCol[1].isTouch = false;

                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldChar">Replaceの機能</param>
        /// <param name="newChar">Replaceの機能</param>
        private void TextUpdate(string oldChar, string newChar)
        {
            if (oldChar != "")
            {
                string sReplaceName;
                for (int i = 0; i < textMesh.Length; i++)
                {
                    sReplaceName = baseList[listUp[i]].Replace(oldChar, newChar);
                    textMesh[i].text = listUp[i].ToString() + ": " + sReplaceName;
                }
            }
            else
            {
                for (int i = 0; i < textMesh.Length; i++)
                {
                    textMesh[i].text = listUp[i].ToString() + ": " + baseList[listUp[i]];
                }
            }
        }
    }
}