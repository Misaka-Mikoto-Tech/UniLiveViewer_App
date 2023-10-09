using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Kari;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class StageLightPresenter : IStartable, IDisposable
    {
        readonly StageLightManager _stageLightManager;
        readonly ISubscriber<SummonedCount> _subscriber;

        readonly CompositeDisposable _disposable;

        [Inject]
        public StageLightPresenter(
            ISubscriber<SummonedCount> subscriber,
            StageLightManager stageLightManager)
        {
            _subscriber = subscriber;
            _stageLightManager = stageLightManager;

            _disposable = new CompositeDisposable();
        }

        void IStartable.Start()
        {
            Debug.Log("Trace: StageLightPresenter.Start");

            _subscriber
                .Subscribe(x => _stageLightManager.OnUpdateSummonedCount(x.Value))
                .AddTo(_disposable);

            Debug.Log("Trace: StageLightPresenter.Start");
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }

}