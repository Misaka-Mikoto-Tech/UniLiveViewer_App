﻿using Cysharp.Threading.Tasks;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class StagePresenter : IStartable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;
        readonly BlackoutCurtain _blackoutCurtain;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public StagePresenter(
            FileAccessManager fileAccessManager,
            BlackoutCurtain blackoutCurtain)
        {
            _fileAccessManager = fileAccessManager;
            _blackoutCurtain = blackoutCurtain;
        }

        void IStartable.Start()
        {
            _fileAccessManager.EndLoadingAsObservable
                .Subscribe(_ => _blackoutCurtain.Ending().Forget())
                .AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
