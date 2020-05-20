using VKClient.Common.Framework;

namespace VKClient.Common.Library.Registration
{
  public class RegistrationAddFriendsViewModel : ViewModelBase, ICompleteable
  {
    private FriendsSearchViewModel _friendsSearchVM = new FriendsSearchViewModel(FriendsSearchMode.Register);

    public FriendsSearchViewModel FriendsSearchVM
    {
      get
      {
        return this._friendsSearchVM;
      }
    }

    public bool IsCompleted
    {
      get
      {
        return true;
      }
    }
  }
}
