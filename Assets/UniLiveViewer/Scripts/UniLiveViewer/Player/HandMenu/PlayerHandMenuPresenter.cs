using MessagePipe;
using System;
using System.Collections.Generic;
using UniLiveViewer.OVRCustom;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player.HandMenu
{
    public class PlayerHandMenuPresenter : IStartable, ILateTickable, IDisposable
    {
        readonly CameraHeightService _cameraHeightService;
        readonly ActorManipulateService _actorManipulateService;
        readonly ItemMaterialSelectionService _itemMaterialSelection;
        readonly PlayerStateManager _playerStateManager;
        readonly List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        readonly CompositeDisposable _disposables = new();
        /// <summary>
        /// 常に1つだけ購読用
        /// </summary>
        readonly SerialDisposable _serialDisposable = new();


        [Inject]
        public PlayerHandMenuPresenter(
            CameraHeightService cameraHeightService,
            ActorManipulateService actorManipulateService,
            ItemMaterialSelectionService itemMaterialSelection,
            PlayerStateManager playerStateManager,
            List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _cameraHeightService = cameraHeightService;
            _actorManipulateService = actorManipulateService;
            _itemMaterialSelection = itemMaterialSelection;
            _playerStateManager = playerStateManager;
            _ovrGrabbers = ovrGrabbers;
        }

        void IStartable.Start()
        {
            foreach (var ovrGrabber in _ovrGrabbers)
            {
                ovrGrabber.GrabbedObj
                    .Subscribe(OnChangeGrabbedObj).AddTo(_disposables);

                ovrGrabber.HandActionStateAsObservable
                    .Where(x => x.Target == HandTargetType.Actor)
                    .Subscribe(x =>
                    {
                        if (x.Action == HandActionState.Grab) _actorManipulateService.ChangeShow(ovrGrabber.HandType, true);
                        if (x.Action == HandActionState.Release) _actorManipulateService.ChangeShow(ovrGrabber.HandType, false);
                    }).AddTo(_disposables);
            }

            _playerStateManager.CameraHeightMenuShow
                .SkipLatestValueOnSubscribe()
                .Subscribe(_ => _cameraHeightService.ChangeShow()).AddTo(_disposables);

            for (int i = 0; i < _playerStateManager.IsItemMaterialSelection.Length; i++)
            {
                int localI = i; // ループの各反復ごとにiの値を新しいローカル変数にコピー,これしないと右手の場合2~3
                _playerStateManager.IsItemMaterialSelection[i]
                    .SkipLatestValueOnSubscribe()
                    .Subscribe(isShow =>
                    {
                        if (isShow)
                        {
                            if (!_ovrGrabbers[localI].GrabbedObj.Value) return;

                            //TODO: 見直す
                            if (!_ovrGrabbers[localI].GrabbedObj.Value.TryGetComponent<DecorationItemInfo>(out var itemInfo)) return;
                            _itemMaterialSelection.ChangeShow(localI, isShow, itemInfo);
                        }
                        else
                        {
                            _itemMaterialSelection.ChangeShow(localI, isShow, null);
                        }
                    }).AddTo(_disposables);

                _playerStateManager.ItemMaterialSelection[localI]
                    .SkipLatestValueOnSubscribe()
                    .Subscribe(current =>
                    {
                        _itemMaterialSelection.SetItemTexture(localI, current);
                    }).AddTo(_disposables);
            }

            _cameraHeightService.Setup();
            _actorManipulateService.Setup();
            _itemMaterialSelection.Setup();
        }


        /// <summary>
        /// 握っている間のみ購読
        /// </summary>
        /// <param name="ovrGrabbableCustom"></param>
        void OnChangeGrabbedObj(OVRGrabbable_Custom ovrGrabbableCustom)
        {
            if (ovrGrabbableCustom == null)
            {
                _serialDisposable.Disposable = null;
            }
            else if (ovrGrabbableCustom.TryGetComponent<Actor.ActorLifetimeScope>(out var actor))
            {
                var actorEntity = actor.Container.Resolve<Actor.IActorEntity>();

                _serialDisposable.Disposable = actorEntity.RootScalar()
                .Subscribe(x =>
                {
                    _actorManipulateService.OnChangeActorSize(x);
                });
            }
            else
            {
                _serialDisposable.Disposable = null;
            }
        }

        void ILateTickable.LateTick()
        {
            _cameraHeightService.OnLateTick();
            _actorManipulateService.OnLateTick();
            _itemMaterialSelection.OnLateTick();
            _playerStateManager.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}