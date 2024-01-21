using Cysharp.Threading.Tasks;
using System.Threading;
using UniRx;
using UnityEngine;

namespace UniLiveViewer.Actor
{
    public interface IActorService
    {
        IReactiveProperty<ActorEntity> ActorEntity();

        IReactiveProperty<bool> Active();

        IReactiveProperty<float> RootScalar();

        IReactiveProperty<ActorState> ActorState();

        /// <summary>
        /// デバッグ用
        /// </summary>
        UniTask EditorOnlySetupAsync(Transform firstParent, CancellationToken cancellation);

        UniTask SetupAsync(Transform firstParent, CancellationToken cancellation);

        void Activate(bool isActive);

        void AddRootScalar(float add);

        void SetState(ActorState setState, Transform overrideTarget);

        void OnTick();

        void Delete();
    }
}
