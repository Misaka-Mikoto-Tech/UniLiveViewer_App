using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class BookPresenter : IStartable, IDisposable
    {
        readonly BookService _bookService;
        readonly SystemSettingsService _systemSettingsService;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public BookPresenter(BookService bookService, SystemSettingsService systemSettingsService)
        {
            _bookService = bookService;
            _systemSettingsService = systemSettingsService;
        }

        void IStartable.Start()
        {
            _systemSettingsService.SystemLanguage
                .Subscribe(_bookService.Initialize)
                .AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
