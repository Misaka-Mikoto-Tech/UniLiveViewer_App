using UnityEngine.Playables;

namespace NanaCiel
{
    public static class TimelineExpansions
    {
        /// <summary>
        /// Stopを挟むと情報が残らないのでcache
        /// TODO: やっぱserviceにする
        /// </summary>
        static double CacheSpeed;

        /// <summary>
        /// 停止状態にする(UIにトリガーを送る為)
        /// 通常Stopを使わせないためのラッパー
        /// </summary>
        /// <param name="playableDirector"></param>
        public static void StopTimeline(this PlayableDirector playableDirector)
        {
            CacheSpeed = playableDirector.playableGraph.GetRootPlayable(0).GetSpeed();
            playableDirector.Stop();//停止状態にする(UIにトリガーを送る為)
        }

        public static void SetSpeedTimeline(this PlayableDirector playableDirector, double speed)
        {
            //if (!playableDirector.playableGraph.IsValid()) return;
            CacheSpeed = speed;
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }

        /// <summary>
        /// TODO: 要確認
        /// 
        /// タイムラインの変更内容を強制的?に反映させる
        /// AnimationClip変更だけ反映されないためリスタートが必要
        /// </summary>
        /// <param name="playableDirector"></param>
        public static void ResumeTimeline(this PlayableDirector playableDirector)
        {
            //再生時間の記録
            var keepTime = playableDirector.time;
            ////初期化して入れ直し(これでいけちゃう謎)
            //_playableDirector.playableAsset = null;
            //_playableDirector.playableAsset = _timelineAsset;

            // clipこれでよさそう
            playableDirector.RebuildGraph();

            //前回の続きを指定
            playableDirector.time = keepTime;

            ////Track情報を更新する
            //TrackList_Update();

            if (playableDirector.timeUpdateMode == DirectorUpdateMode.GameTime)
            {
                playableDirector.Play();
                playableDirector.SetSpeedTimeline(CacheSpeed);//Play後に再適用必須
            }
            else if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //1f更新
                playableDirector.Evaluate();
            }
        }
    }
}
