using System.Collections.Generic;
using UniLiveViewer.Actor;
using UniLiveViewer.Actor.Option;
using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/ActorLifetimeScopeSetting", fileName = "ActorLifetimeScopeSetting")]
    public class ActorLifetimeScopeSetting : ScriptableObject
    {
        /// <summary>
        /// とりあえずUnityちゃん
        /// </summary>
        public List<ActorLifetimeScope> FBXActorLifetimeScopePrefab;
        /// <summary>
        /// VRMは空の共通テンプレ
        /// </summary>
        public ActorLifetimeScope VrmActorLifetimeScopePrefab;
        /// <summary>
        /// VRMは空の共通テンプレ
        /// </summary>
        public ActorLifetimeScope Vrm10ActorLifetimeScopePrefab;

        public ActorOptionLifetimeScope OptionLifetimeScope;
    }
}