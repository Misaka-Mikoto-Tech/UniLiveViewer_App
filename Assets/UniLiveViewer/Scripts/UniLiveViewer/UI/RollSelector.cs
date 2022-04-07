using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    //����Ă݂����p�~������
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
            //�O��f�[�^���폜
            current = 0;
            if (listUp.Count > 0) listUp.Clear();
            if (baseList.Count > 0) baseList.Clear();

            //�R�s�[
            baseList = _list;

            //�����̃��X�g�A�b�v���镪��ݒ�
            listUp.Add(baseList.Count - 2 - current);
            listUp.Add(baseList.Count - 1 - current);
            listUp.Add(current);
            listUp.Add(current + 1);
            listUp.Add(current + 2);

            //�e�L�X�g�ɔ��f
            TextUpdate(" (Instance)", "");
        }

        private IEnumerator RollUpdate()
        {
            int stateDefault = Animator.StringToHash("Default");

            //�y�[�W���J����Ă����
            while (transform)
            {
                //�G�ꂽ�R���C�_�[���ɉ�]�A�j���[�V����
                if (touchCol[0].isTouch)
                {
                    current--;
                    if (current < 0) current = baseList.Count - 1;

                    //�C�x���g
                    if (onTouch != null) onTouch();

                    anime.SetBool("Roll_Up", true);

                    yield return null;
                    anime.SetBool("Roll_Up", false);

                    listUp.RemoveAt(listUp.Count - 1);//�������폜

                    if (current - 2 < 0)
                    {
                        listUp.Insert(0, baseList.Count - (2 - current));//�擪�ɒǉ�
                    }
                    else
                    {
                        listUp.Insert(0, current - 2);//�擪�ɒǉ�
                    }

                    yield return new WaitForSeconds(0.01f);//AnimatorState���m���ɐ؂�ւ��܂ŏ����҂�
                    while (anime.GetCurrentAnimatorStateInfo(0).shortNameHash != stateDefault)
                    {
                        yield return null;
                    }
                    //�e�L�X�g�ɔ��f
                    TextUpdate(" (Instance)", "");

                    touchCol[0].isTouch = false;
                }
                else if (touchCol[1].isTouch)
                {
                    current++;
                    if (current >= baseList.Count) current = 0;

                    //�C�x���g
                    if (onTouch != null) onTouch();

                    anime.SetBool("Roll_Down", true);

                    yield return null;
                    anime.SetBool("Roll_Down", false);

                    listUp.RemoveAt(0);//�擪���폜
                    if (current + 2 >= baseList.Count)
                    {
                        listUp.Add(current + 2 - baseList.Count);//�����ɒǉ�
                    }
                    else
                    {
                        listUp.Add(current + 2);//�����ɒǉ�
                    }

                    yield return new WaitForSeconds(0.01f);//AnimatorState���m���ɐ؂�ւ��܂ŏ����҂�
                    while (anime.GetCurrentAnimatorStateInfo(0).shortNameHash != stateDefault)
                    {
                        yield return null;
                    }

                    //�e�L�X�g�ɔ��f
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
        /// <param name="oldChar">Replace�̋@�\</param>
        /// <param name="newChar">Replace�̋@�\</param>
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