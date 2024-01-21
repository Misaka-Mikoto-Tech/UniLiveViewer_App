using System.Collections.Generic;
using UniLiveViewer.Actor;
using UniLiveViewer.Timeline;
using UniLiveViewer.ValueObject;

namespace UniLiveViewer.Menu
{
    public class ActorRegisterService
    {
        public IReadOnlyList<RegisterData> RegisterDataFBX => _registerDataFBX;
        List<RegisterData> _registerDataFBX = new();

        public IReadOnlyList<RegisterData> RegisterDataVRM => _registerDataVRM;
        List<RegisterData> _registerDataVRM = new();

        int _indexFBX = 0;
        int _indexVRM = 0;

        public ActorRegisterService()
        {
        }

        /// <summary>
        /// 登録のみ、ActorIdが確定
        /// </summary>
        /// <returns></returns>
        public void RegisterFBX(string name)
        {
            var actorId = new ActorId(ActorType.FBX, _indexFBX);
            var dummy = new VRMLoadData(name);//これ辞めたい
            _registerDataFBX.Add(new RegisterData(actorId, dummy));
            _indexFBX++;
        }

        public bool TryGetRegisterDataFBX(int index, out RegisterData data)
        {
            if (index < 0 || _registerDataFBX.Count <= index)
            {
                data = null;
                return false;
            }

            data = _registerDataFBX[index];
            return true;
        }

        /// <summary>
        /// 登録のみ、ActorIdが確定
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void RegisterVRM(VRMLoadData data)
        {
            var actorId = new ActorId(ActorType.VRM, _indexVRM);
            _registerDataVRM.Add(new RegisterData(actorId, data));
            _indexVRM++;
        }

        public bool TryGetRegisterDataVRM(int index, out RegisterData data)
        {
            if (index < 0 || _registerDataVRM.Count <= index)
            {
                data = null;
                return false;
            }

            data = _registerDataVRM[index];
            return true;
        }

        public void RemoveVRM(int index)
        {
            _registerDataVRM[index] = null;
            _registerDataVRM.RemoveAt(index);
        }
    }

    public class RegisterData
    {
        public ActorId Id { get; }
        public VRMLoadData VRMLoadData { get; }

        public RegisterData(ActorId id, VRMLoadData vrmLoadData)
        {
            Id = id;
            VRMLoadData = vrmLoadData;
        }
    }
}