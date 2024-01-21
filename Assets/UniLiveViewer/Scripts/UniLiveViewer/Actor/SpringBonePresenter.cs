using UniLiveViewer.Player;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class SpringBonePresenter : ITickable
    {
        readonly IActorService _actorEntityService;

        [Inject]
        public SpringBonePresenter(
            IActorService actorEntityService)
        {
            _actorEntityService = actorEntityService;
        }

        void ITickable.Tick()
        {
            if (_actorEntityService.ActorEntity().Value == null) return;

            //揺れもの接触振動
            var charaInfoData = _actorEntityService.ActorEntity().Value.CharaInfoData;
            if (charaInfoData.ActorType != ActorType.VRM) return;

            foreach (var springBone in _actorEntityService.ActorEntity().Value.SpringBoneList)
            {
                if (springBone.isHit_Any == false) continue;
                if (springBone.isLeft_Any)
                {
                    ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, charaInfoData.power, charaInfoData.time);
                }
                if (springBone.isRight_Any)
                {
                    ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, charaInfoData.power, charaInfoData.time);
                }
                break;
            }
        }

        // TODO: 生成時とPrefab時に切り替えないといけないみたい、最終的に使うか微妙
        void SetEnabelSpringBones(bool isEnabel)
        {
            //foreach (var e in _springBoneList)
            //{
            //    e.enabled = isEnabel;
            //}
        }
    }
}
