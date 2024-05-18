using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class BookService
    {
        readonly BookSetting _bookSetting;
        readonly BookAnchor _bookAnchor;

        [Inject]
        public BookService(BookSetting bookSetting, BookAnchor bookAnchor)
        {
            _bookSetting = bookSetting;
            _bookAnchor = bookAnchor;
        }

        public void Initialize()
        {
            var index = FileReadAndWriteUtility.UserProfile.LanguageCode - 1;
            if (index == 0)
            {
                GameObject.Instantiate(_bookSetting.PrefabJP, _bookAnchor.transform);
            }
            else
            {
                GameObject.Instantiate(_bookSetting.PrefabEN, _bookAnchor.transform);
            }
        }

        public void ChangeOpenClose()
        {
            _bookAnchor.gameObject.SetActive(!_bookAnchor.gameObject.activeSelf);
        }
    }
}