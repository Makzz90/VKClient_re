using System.Collections.ObjectModel;
using VKClient.Common.Framework;

namespace VKClient.Common.Emoji
{
  public class EmojiPanelViewModel : ViewModelBase
  {
    private ObservableCollection<object> _items = new ObservableCollection<object>();

    public ObservableCollection<object> Items
    {
      get
      {
        return this._items;
      }
    }
  }
}
