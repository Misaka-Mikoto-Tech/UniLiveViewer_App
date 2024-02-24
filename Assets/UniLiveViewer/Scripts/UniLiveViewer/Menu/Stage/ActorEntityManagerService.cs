using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Timeline;
using UniLiveViewer.ValueObject;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class ActorEntityManagerService
    {
        /// <summary>
        /// 登録された件数（生成の可否は不定）
        /// </summary>
        public int NumRegisteredFBX => _actorRegisterService.RegisterDataFBX.Count;
        readonly List<ActorData> _fbxList = new();
        /// <summary>
        /// 登録された件数（生成の可否は不定）
        /// </summary>
        public int NumRegisteredVRM => _actorRegisterService.RegisterDataVRM.Count;
        readonly List<ActorData> _vrmList = new();

        /// <summary>
        /// 召喚時などは不在なのでnull
        /// </summary>
        public bool TryGetCurrentInstaceID(out InstanceId instanceId)
        {
            if (_currentInstaceID == null)
            {
                instanceId = null;
                return false;
            }
            else
            {
                instanceId = _currentInstaceID;
                return true;
            }
        }
        InstanceId _currentInstaceID;

        readonly IPublisher<AllActorOperationMessage> _allPublisher;
        readonly IPublisher<ActorOperationMessage> _publisher;
        readonly ActorLifetimeScopeSetting _actorSetting;
        readonly ActorRegisterService _actorRegisterService;
        readonly ActorEntityFactory _actorEntityFactory;

        [Inject]
        public ActorEntityManagerService(
            IPublisher<AllActorOperationMessage> allPublisher,
            IPublisher<ActorOperationMessage> publisher,
            ActorLifetimeScopeSetting actorSetting,
            ActorRegisterService actorRegisterService,
            ActorEntityFactory actorEntityFactory)
        {
            _allPublisher = allPublisher;
            _publisher = publisher;
            _actorSetting = actorSetting;
            _actorRegisterService = actorRegisterService;
            _actorEntityFactory = actorEntityFactory;
        }

        public string[] FbxViewNames =>
                _actorRegisterService.RegisterDataFBX.Select(x => x.VRMLoadData.FileName).ToArray();

        public string[] VRMViewNames =>
                _actorRegisterService.RegisterDataVRM.Select(x => x.VRMLoadData.FileName).ToArray();

        public IActorEntity EditorOnly_CurrentActorService(InstanceId instanceId)
        {

            var data = _fbxList.Where(x => x != null).FirstOrDefault(x => x.GetInstanceId() == instanceId);
            if (data != null) return data.ActorEntity;

            data = _vrmList.Where(x => x != null).FirstOrDefault(x => x.GetInstanceId() == instanceId);
            if (data != null) return data.ActorEntity;

            return null;
        }

        public void RegisterFBX()
        {
            foreach (var data in _actorSetting.FBXActorLifetimeScopePrefab)
            {
                _actorRegisterService.RegisterFBX(data.ActorName);
                _fbxList.Add(null);
            }
        }

        public void FastRegisterVRM()
        {
            var dummy = new VRMLoadData(MenuConstants.LoadVRM);
            _actorRegisterService.RegisterVRM(dummy);
            if (_actorRegisterService.TryGetRegisterDataVRM(0, out var registerData))
            {
                _vrmList.Add(new ActorData(registerData, null));
            }
        }

        public void RegisterVRM(VRMLoadData data)
        {
            _actorRegisterService.RegisterVRM(data);
            _vrmList.Add(null);
        }

        public void SendAllActorDisableMessage()
        {
            var message = new AllActorOperationMessage(ActorState.MINIATURE, ActorCommand.INACTIVE);
            _allPublisher.Publish(message);
        }

        public async UniTask<IActorEntity> ActiveFBXAsync(int index, CancellationToken cancellation)
        {
            if (index < 0 || _fbxList.Count <= index)
            {
                Debug.LogWarning("indexどこかでズレた疑惑（ここ来たらおかしい）");
                return null;
            }

            SendAllActorDisableMessage();

            // 既出ならアクティブ化のみ
            if (_fbxList[index] != null)
            {
                _currentInstaceID = _fbxList[index].GetInstanceId();
                var message = new ActorOperationMessage(_currentInstaceID, ActorCommand.ACTIVE);
                _publisher.Publish(message);
                return _fbxList[index].ActorEntity;
            }
            // 生成(アクティブ状態で生成される)
            else
            {
                if (!_actorRegisterService.TryGetRegisterDataFBX(index, out var registerData))
                {
                    Debug.LogWarning("indexどこかでズレた疑惑（ここ来たらおかしい）");
                    return null;
                }

                var actor = await _actorEntityFactory.GenerateFBXAsync(registerData, cancellation);
                _fbxList[index] = new ActorData(registerData, actor);
                _currentInstaceID = _fbxList[index].GetInstanceId();
                return _fbxList[index].ActorEntity;
            }
        }

        public async UniTask<IActorEntity> ActiveVRMAsync(int index, CancellationToken cancellation)
        {
            if (index < 0 || _vrmList.Count <= index)
            {
                Debug.LogWarning("indexどこかでズレた疑惑（ここ来たらおかしい）");
                return null;
            }

            SendAllActorDisableMessage();

            // 既出ならアクティブ化のみ
            if (_vrmList[index] != null)
            {
                _currentInstaceID = _vrmList[index].GetInstanceId();
                var message = new ActorOperationMessage(_currentInstaceID, ActorCommand.ACTIVE);
                _publisher.Publish(message);
                return _vrmList[index].ActorEntity;
            }
            // 生成(アクティブ状態で生成される)
            else
            {
                if (!_actorRegisterService.TryGetRegisterDataVRM(index, out var registerData))
                {
                    Debug.LogWarning("indexどこかでズレた疑惑（ここ来たらおかしい）");
                    return null;
                }

                var actor = await _actorEntityFactory.GenerateVRMAsync(registerData, cancellation);
                _vrmList[index] = new ActorData(registerData, actor);
                _currentInstaceID = _vrmList[index].GetInstanceId();
                return _vrmList[index].ActorEntity;
            }
        }

        /// <summary>
        /// 召喚成功時を想定（管理から外す）
        /// </summary>
        /// <param name="index"></param>
        public void RemoveFBX(int index)
        {
            _fbxList[index] = null;
            _currentInstaceID = null;
        }

        /// <summary>
        /// 召喚成功時を想定（管理から外す）
        /// </summary>
        /// <param name="index"></param>
        public void RemoveVRM(int index)
        {
            _vrmList[index] = null;
            _currentInstaceID = null;
        }

        /// <summary>
        /// 登録情報とinstanceを削除する
        /// </summary>
        /// <param name="index"></param>
        public void DeleteVRM(int index)
        {
            _actorRegisterService.RemoveVRM(index);
            _vrmList[index].Dispose();
            _vrmList[index] = null;
            _vrmList.RemoveAt(index);
            _currentInstaceID = null;
        }

        class ActorData : IDisposable
        {
            public RegisterData RegisterData { get; }
            public IActorEntity ActorEntity { get; }
            readonly ActorLifetimeScope _actorLifetimeScope;

            public ActorData(RegisterData data, ActorLifetimeScope actorLifetimeScope)
            {
                RegisterData = data;
                if (actorLifetimeScope == null) return;
                _actorLifetimeScope = actorLifetimeScope;
                ActorEntity = actorLifetimeScope.Container.Resolve<IActorEntity>();
            }

            public InstanceId GetInstanceId()
            {
                if (_actorLifetimeScope == null) return null;
                return _actorLifetimeScope.InstanceId;
            }

            /// <summary>
            /// instanceも削除される
            /// </summary>
            public void Dispose()
            {
                if (_actorLifetimeScope == null) return;
                GameObject.Destroy(_actorLifetimeScope.gameObject);
            }
        }
    }
}