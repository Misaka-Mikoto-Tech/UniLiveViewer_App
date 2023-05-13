using UnityEngine;
using UnityEngine.Playables;

namespace UniLiveViewer
{
    // NOTE:controller側がカオスなので一端雑に分離
    public class TimelineInfo : MonoBehaviour
    {
        TimelineController _timeline;

        public PlayableDirector GetPlayableDirector => _playableDirector;
        PlayableDirector _playableDirector;

        /// <summary>
        /// トラックから指定キャラを取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CharaController GetCharacter(int index) => _timeline.BindCharaMap[index];

        /// <summary>
        /// timeline上でのキャラ上限、今6決め打ち
        /// </summary>
        /// <returns></returns>
        public int CharacterCount => _timeline.BindCharaMap.Count;

        /// <summary>
        /// フィールドの現在キャラ数
        /// </summary>
        public int FieldCharaCount => _timeline.FieldCharaCount;

        /// <summary>
        /// フィールドに存在できる最大キャラ数
        /// </summary>
        public int MaxFieldChara => _timeline.MaxFieldChara;

        public bool IsManualMode => _playableDirector.timeUpdateMode == DirectorUpdateMode.Manual;

        void Awake()
        {
            _timeline = GetComponent<TimelineController>();
            _playableDirector = GetComponent<PlayableDirector>();
        }
    }
}