using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 足音service
    /// </summary>
    public class FootstepService
    {
        /// <summary>
        /// 要調整
        /// </summary>
        const float FootRay = 0.2f;

        List<FoodMap> _foodMap = new();
        ActorEntity _actorEntity;

        readonly AudioSource _footstepsAudio;
        readonly ActorOptionSetting _setting;

        [Inject]
        public FootstepService(
            AudioSource footstepsAudio,
            ActorOptionSetting setting)
        {
            _footstepsAudio = footstepsAudio;
            _setting = setting;
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
            if (actorEntity == null) return;
            _foodMap.Add(new FoodMap(actorEntity.BoneMap[HumanBodyBones.LeftFoot]));
            _foodMap.Add(new FoodMap(actorEntity.BoneMap[HumanBodyBones.RightFoot]));
        }

        public void OnFixedTick()
        {
            if (_actorEntity == null) return;

            if (!FootstepAudio.IsFootstepAudio) return;
            if (SceneChangeService.GetSceneType != SceneType.GYMNASIUM) return;

            for (int i = 0; i < _foodMap.Count; i++)
            {
                CheckFootContact(_foodMap[i]);
            }
        }

        /// <summary>
        /// 足と床の衝突判定
        /// </summary>
        void CheckFootContact(FoodMap foodMap)
        {
            if (Physics.Raycast(foodMap.FootBone.position, Vector3.down, out var hitCollider, FootRay, Constants.LayerMaskStageFloor))
            {
                var isHit = hitCollider.collider != null;
                if (isHit == foodMap.IsHitCache) return;
                foodMap.SetHit(isHit);

                if (!isHit) return;
                PlaySound(hitCollider.point);
            }
            else
            {
                if (!foodMap.IsHitCache) return;
                foodMap.SetHit(false);
            }
        }

        void PlaySound(Vector3 hitPoint)
        {
            _footstepsAudio.transform.position = hitPoint;
            var index = UnityEngine.Random.Range(0, _setting.FootstepAudioClips.Count);
            _footstepsAudio.PlayOneShot(_setting.FootstepAudioClips[index]);
        }

        public class FoodMap
        {
            public Transform FootBone { get; private set; }

            public bool IsHitCache { get; private set; }


            public FoodMap(Transform footBone)
            {
                FootBone = footBone;
                IsHitCache = false;
            }

            public void SetHit(bool isHit)
            {
                IsHitCache = isHit;
            }
        }
    }
}
