using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class BookPresenter : IStartable
    {
        readonly BookService _bookService;

        [Inject]
        public BookPresenter(BookService bookService)
        {
            _bookService = bookService;
        }

        void IStartable.Start()
        {
            _bookService.Initialize();
        }
    }
}
