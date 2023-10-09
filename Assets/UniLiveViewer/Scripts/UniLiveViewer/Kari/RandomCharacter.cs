using UnityEngine;

namespace UniLiveViewer.Kari
{
    public class RandomCharacter : MonoBehaviour
    {
        [SerializeField] Transform[] _chara = new Transform[3];

        void Start()
        {
            //randomにキャラ変更
            int r = Random.Range(0, 3);
            for (int i = 0; i < 3; i++)
            {
                if (r == i) _chara[i].gameObject.SetActive(true);
                else _chara[i].gameObject.SetActive(false);
            }
        }
    }
}