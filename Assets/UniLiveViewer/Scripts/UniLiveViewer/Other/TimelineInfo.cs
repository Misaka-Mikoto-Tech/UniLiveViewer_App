using UnityEngine;
using UnityEngine.Playables;

namespace UniLiveViewer
{
    // NOTE:controller側がカオスなので一端雑に分離
    public class TimelineInfo : MonoBehaviour
    {
        public string PortalBaseAniTrack => _timeline.PortalBaseAniTrack;

        public PlayableDirector GetPlayableDirector => _playableDirector;
        PlayableDirector _playableDirector;

        /// <summary>
        /// トラックから指定キャラを取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CharaController GetCharacter(int index)
        {
            return _timeline.GetCharacter(index);
        }

        /// <summary>
        /// timeline上でのキャラ上限、今6決め打ち
        /// </summary>
        /// <returns></returns>
        public int CharacterCount()
        {
            return _timeline.CharacterCount();
        }

        /// <summary>
        /// フィールドの現在キャラ数
        /// </summary>
        public int FieldCharaCount => _timeline.FieldCharaCount;

        /// <summary>
        /// フィールドに存在できる最大キャラ数
        /// </summary>
        public int MaxFieldChara => _timeline.MaxFieldChara;

        /// <summary>
        /// ポータル枠にキャラが存在するか
        /// </summary>
        /// <returns></returns>
        public bool IsPortalChara() { return _timeline.IsPortalChara(); }

        TimelineController _timeline;

        void Awake()
        {
            _timeline = GetComponent<TimelineController>();
            _playableDirector = GetComponent<PlayableDirector>();
        }

        public bool isManualMode()
        {
            return _playableDirector.timeUpdateMode == DirectorUpdateMode.Manual;
        }
    }
}