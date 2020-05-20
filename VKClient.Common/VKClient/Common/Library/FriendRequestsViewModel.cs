using System;
using System.Linq;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class FriendRequestsViewModel : ViewModelBase, IHandle<FriendRequestAcceptedDeclined>, IHandle, ICollectionDataProvider<FriendRequests, FriendRequestData>
  {
    public FriendRequestsUC ParentFriendRequestsUC;
    private readonly bool _areSuggestedFriends;

    public string Title { get; set; }

    public GenericCollectionViewModel<FriendRequests, FriendRequestData> FriendRequestsVM { get; set; }

    public Func<FriendRequests, ListWithCount<FriendRequestData>> ConverterFunc
    {
      get
      {
        return (Func<FriendRequests, ListWithCount<FriendRequestData>>) (response =>
        {
          ListWithCount<FriendRequestData> listWithCount = new ListWithCount<FriendRequestData>();
          foreach (FriendRequest request in response.requests)
          {
            FriendRequestData friendRequestData = new FriendRequestData();
            friendRequestData.Model = request;
            User[] array = response.profiles.ToArray();
            friendRequestData.Profiles = array;
            int num = response.are_suggested_friends ? 1 : 0;
            friendRequestData.IsSuggestedFriend = num != 0;
            friendRequestData.ParentViewModel = (object) this;
            FriendRequestData data = friendRequestData;
            data.Model.RequestHandledAction = (Action<FriendRequests>) (i =>
            {
              this.FriendRequestsVM.Delete(data);
              if (this.ParentFriendRequestsUC == null)
                return;
              ((FriendsViewModel) this.ParentFriendRequestsUC.DataContext).RequestsViewModel = i;
            });
            listWithCount.List.Add(data);
          }
          listWithCount.TotalCount = response.count;
          CountersManager.Current.Counters.friends = response.menu_counter;
          EventAggregator.Current.Publish((object) new CountersChanged(CountersManager.Current.Counters));
          if (this.ParentFriendRequestsUC != null)
            this.ParentFriendRequestsUC.UpdateDataView(response);
          return listWithCount;
        });
      }
    }

    public FriendRequestsViewModel(bool areSuggestedFriends = false)
    {
      EventAggregator.Current.Subscribe((object) this);
      this.FriendRequestsVM = new GenericCollectionViewModel<FriendRequests, FriendRequestData>((ICollectionDataProvider<FriendRequests, FriendRequestData>) this)
      {
        NeedCollectionCountBeforeFullyLoading = true
      };
      this._areSuggestedFriends = areSuggestedFriends;
      this.Title = !areSuggestedFriends ? CommonResources.FriendRequestsPage_Title : CommonResources.Friends_SuggestedFriends;
    }

    public void Handle(FriendRequestAcceptedDeclined message)
    {
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        FriendRequestData friendRequestData = this.FriendRequestsVM.Collection.FirstOrDefault<FriendRequestData>((Func<FriendRequestData, bool>) (request => request.Model.user_id == message.UserId));
        if (friendRequestData == null)
          return;
        this.FriendRequestsVM.Delete(friendRequestData);
      }));
    }

    public void GetData(GenericCollectionViewModel<FriendRequests, FriendRequestData> caller, int offset, int count, Action<BackendResult<FriendRequests, ResultCode>> callback)
    {
      UsersService.Instance.GetFriendRequests(this._areSuggestedFriends, offset, count, callback);
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<FriendRequests, FriendRequestData> caller, int count)
    {
      if (count <= 0)
      {
        if (this._areSuggestedFriends)
          return CommonResources.NoSuggestedFriends;
        return CommonResources.NoFriendRequestsFrm;
      }
      if (!this._areSuggestedFriends)
        return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OneFriendRequestFrm, CommonResources.TwoFourFriendRequestsFrm, CommonResources.FiveFriendRequestsFrm, true, null, false);
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.SuggestedFriendOneFrm, CommonResources.SuggestedFriendTwoFrm, CommonResources.SuggestedFriendFiveFrm, true, null, false);
    }
  }
}
