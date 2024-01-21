using NanaCiel;
using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.Actor;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// TimelineのAnimatorオブジェクト紐づけのみを管理するservice
    /// 
    /// </summary>
    public class PlayableBinderService
    {
        public IReadOnlyReactiveProperty<int> StageActorCount => _stageActorCount;
        readonly ReactiveProperty<int> _stageActorCount = new();

        /// <summary>
        /// 新規バインド
        /// </summary>
        public IObservable<Unit> NewBindingAsObservable => _newBindingStream;
        readonly Subject<Unit> _newBindingStream = new();

        /// <summary>
        /// 別トラックにバインド
        /// （現状はPortal→Stageのみ想定）
        /// </summary>
        public IObservable<Unit> BindingToAsObservable => _bindingToStream;
        readonly Subject<Unit> _bindingToStream = new();

        public IReadOnlyList<BindingData> BindingData => _bindingData;
        readonly List<BindingData> _bindingData = Enumerable.Repeat<BindingData>(null, 6).ToList();

        readonly PlayableDirector _playableDirector;

        [Inject]
        public PlayableBinderService(PlayableDirector playableDirector)
        {
            _playableDirector = playableDirector;
        }

        /// <summary>
        /// NOTE: 先客がいても上書きBind
        /// ActorEntityManagerService側で非Currentは非アクティブにしてるので削除なども不要
        /// </summary>
        /// <param name="actorEntityService"></param>
        public void BindingNewActor(InstanceId instanceId, IActorService actorService)
        {
            var outputs = _playableDirector.playableAsset.outputs;
            var baseName = TimelineConstants.TrackNames[TimelineConstants.PortalIndex];
            var playableBinding = outputs.FirstOrDefault(x => x.streamName == baseName);
            if (playableBinding.streamName == "")
            {
                Debug.Log("System : PlayableBinding名が不一致");
                return;
            }

            //// ポータル枠を削除しておく
            //ClearCaracter();

            //オブジェクトをバインドする
            _playableDirector.SetGenericBinding(playableBinding.sourceObject, actorService.ActorEntity().Value.GetAnimator);

            var data = new BindingData(playableBinding.sourceObject, baseName, instanceId, actorService);
            _bindingData[TimelineConstants.PortalIndex] = data;
            _playableDirector.ResumeTimeline();

            _newBindingStream.OnNext(Unit.Default);
        }

        /// <summary>
        /// Editor拡張のDebug用
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="eulerAngles"></param>
        public InstanceId EditorOnly_TrySwitchTrackBinding()
        {
            if (_bindingData[TimelineConstants.PortalIndex] == null) return null;
            var data = _bindingData[TimelineConstants.PortalIndex];
            TrySwitchTrackBinding(data.InstanceId, data.ActorService);
            return data.InstanceId;
        }

        /// <summary>
        /// 別トラックにバインドを試みる（自動的にポータル以外の空き枠）
        /// </summary>
        /// <param name="instanceId"></param>
        /// <param name="actorService"></param>
        /// <returns></returns>
        public bool TrySwitchTrackBinding(InstanceId instanceId, IActorService actorService)
        {
            // 空があるかチェック、無ければ失敗
            var freePlayable = TryGetFreePlayable();
            if (freePlayable == null) return false;

            Unbind(instanceId);
            BindingTo(freePlayable.Value, instanceId, actorService);

            _stageActorCount.Value += 1;
            _bindingToStream.OnNext(Unit.Default);
            return true;
        }

        /// <summary>
        /// 未バインドのSourceObjectを取得
        /// </summary>
        /// <returns></returns>
        public PlayableBinding? TryGetFreePlayable()
        {
            var usedStreamNames = _bindingData.Where(x => x != null).Select(x => x.StreamName);
            var names = TimelineConstants.TrackNames;
            for (int i = 0; i < names.Length; i++)
            {
                if (usedStreamNames.Contains(names[i])) continue;
                // 未使用枠
                var outputs = _playableDirector.playableAsset.outputs;
                var playableBinding = outputs.FirstOrDefault(x => x.streamName == names[i]);
                return playableBinding;
            }
            //空き無し
            return null;
        }

        /// <summary>
        /// 指定先にバインドする
        /// バインド先に先客はいない前提とする
        /// </summary>
        /// <param name="bindingSourceObject"></param>
        /// <param name="actorEntityService"></param>
        void BindingTo(PlayableBinding playableBinding, InstanceId instanceId, IActorService actorService)
        {
            if (actorService.ActorEntity().Value.GetAnimator == null) return;

            _playableDirector.SetGenericBinding(playableBinding.sourceObject, actorService.ActorEntity().Value.GetAnimator);
            var data = new BindingData(playableBinding.sourceObject, playableBinding.streamName, instanceId, actorService);

            var index = Array.IndexOf(TimelineConstants.TrackNames, playableBinding.streamName);
            if (index < 0) return;
            _bindingData[index] = data;
        }

        public void PortalUnbind()
        {
            var data = _bindingData[TimelineConstants.PortalIndex];
            if (data == null) return;
            Unbind(data.InstanceId);
            _playableDirector.ResumeTimeline();
        }

        public void OnDeleteAllActor()
        {
            // index0以外のすべての要素を削除(ポータルのみ残す)
            for (int i = 1; i < _bindingData.Count; i++)
            {
                _bindingData[i] = null;
            }
            _stageActorCount.Value = 0;
        }

        public void OnDeleteActor(InstanceId instanceId)
        {
            Unbind(instanceId);
            _playableDirector.ResumeTimeline();
            _stageActorCount.Value -= 1;
        }

        /// <summary>
        /// 解除のみ（削除は勝手にやって）
        /// </summary>
        /// <param name="actorEntity"></param>
        void Unbind(InstanceId instanceId)
        {
            var bindingData = _bindingData.Where(x => x?.InstanceId == instanceId).FirstOrDefault();
            if (bindingData == null)
            {
                Debug.LogWarning("Not originally bound");
                return;
            }

            // 必須ではないがnullバインドしておく、えらい！
            _playableDirector.SetGenericBinding(bindingData.BindingSource, null);
            var index = Array.IndexOf(TimelineConstants.TrackNames, bindingData.StreamName);
            if (index < 0) return;
            _bindingData[index] = null;
        }
    }

    public class BindingData
    {
        public UnityEngine.Object BindingSource { get; }

        public string StreamName { get; }

        public InstanceId InstanceId { get; }

        public IActorService ActorService { get; }

        public BindingData(UnityEngine.Object bindingSourceObject, string streamName, InstanceId instanceId, IActorService actorService)
        {
            BindingSource = bindingSourceObject;
            StreamName = streamName;
            InstanceId = instanceId;
            ActorService = actorService;
        }
    }
}