using System.Collections.Generic;
using UniLiveViewer.Actor;
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
        public void RegisterFBX(string fileName)
        {
            var actorId = new ActorId(ActorType.FBX, _indexFBX);
            _registerDataFBX.Add(new RegisterData(actorId, fileName));
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
        public void RegisterVRM(string fileName)
        {
            var actorId = new ActorId(ActorType.VRM, _indexVRM);
            var loadVrmAsMode10 = FileReadAndWriteUtility.UserProfile.IsVRM10;
            _registerDataVRM.Add(new RegisterData(actorId, fileName, loadVrmAsMode10));
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
}