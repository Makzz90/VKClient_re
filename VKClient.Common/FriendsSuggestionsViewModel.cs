using VKClient.Common.Framework;
using VKClient.Common.Library;

namespace VKClient.Common
{
  public class FriendsSuggestionsViewModel : ViewModelBase
  {
    private FriendsSearchViewModel _friendsSearchVM = new FriendsSearchViewModel(FriendsSearchMode.Default);

    public FriendsSearchViewModel FriendsSearchVM
    {
      get
      {
        return this._friendsSearchVM;
      }
    }
  }
}
