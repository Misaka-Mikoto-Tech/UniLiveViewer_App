using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 足音service
    /// </summary>
    public class FootStepService
    {
        /// <summary>
        /// 要調整
        /// </summary>
        const float FootRay = 0.2f;

        List<FootMap> _footMap = new();
        ActorEntity _actorEntity;

        readonly AudioSourceService _audioSourceService;
        readonly AudioFootStepsDataSet _audioFootStepsDataSet;

        [Inject]
        public FootStepService(
            AudioSourceService audioSourceService,
            AudioClipSettings setting)
        {
            _audioSourceService = audioSourceService;
            _audioFootStepsDataSet = setting.GetSceneAudioDataSet(SceneChangeService.GetSceneType).AudioFootStepsDataSet;
        }

        public void SetVolume(float volume)
        {
            _audioSourceService.SetVolume(volume);
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            _actorEntity = actorEntity;
            if (actorEntity == null) return;
            _footMap.Add(new FootMap(actorEntity.BoneMap[HumanBodyBones.LeftFoot]));
            _footMap.Add(new FootMap(actorEntity.BoneMap[HumanBodyBones.RightFoot]));
        }

        public void OnFixedTick()
        {
            if (_actorEntity == null) return;

            //TODO: ステージ別で音
            //if (SceneChangeService.GetSceneType != SceneType.GYMNASIUM) return;

            for (int i = 0; i < _footMap.Count; i++)
            {
                CheckFootContact(_footMap[i]);
            }
        }

        /// <summary>
        /// 足と床の衝突判定
        /// </summary>
        void CheckFootContact(FootMap footMap)
        {
            if (Physics.Raycast(footMap.FootBone.position, Vector3.down, out var hitCollider, FootRay, Constants.LayerMaskStageFloor))
            {
                var isHit = hitCollider.collider != null;
                if (isHit == footMap.IsHitCache) return;
                footMap.SetHit(isHit);

                if (!isHit) return;
                PlaySound(hitCollider.point);
            }
            else
            {
                if (!footMap.IsHitCache) return;
                footMap.SetHit(false);
            }
        }

        void PlaySound(Vector3 hitPoint)
        {
            _audioSourceService.transform.position = hitPoint;
            var index = UnityEngine.Random.Range(0, _audioFootStepsDataSet.AudioClip.Count);
            _audioSourceService.PlayOneShot(_audioFootStepsDataSet.AudioClip[index]);
        }

        public class FootMap
        {
            public Transform FootBone { get; private set; }
            public bool IsHitCache { get; private set; }

            public FootMap(Transform footBone)
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
