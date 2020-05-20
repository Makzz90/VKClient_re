using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library.FriendsImport;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Library
{
  public class FriendsSearchViewModel : ViewModelBase, ICollectionDataProvider<VKList<User>, SubscriptionItemHeader>
  {
    private Visibility _listHeaderVisibility = Visibility.Collapsed;
    private readonly FriendsSearchMode _mode;
    private readonly GenericCollectionViewModel<VKList<User>, SubscriptionItemHeader> _searchVM;
    private bool _contactsLoaded;
    private string _recommendationsNextFrom;
    private bool _allowSendContacts;

    public GenericCollectionViewModel<VKList<User>, SubscriptionItemHeader> SearchVM
    {
      get
      {
        return this._searchVM;
      }
    }

    public string ListHeaderTitle
    {
      get
      {
        switch (this._mode)
        {
          case FriendsSearchMode.Default:
            return CommonResources.PeopleYouMayKnow;
          case FriendsSearchMode.Register:
            return CommonResources.FriendsFromContacts;
          default:
            return "";
        }
      }
    }

    public Visibility ListHeaderVisibility
    {
      get
      {
        return this._listHeaderVisibility;
      }
      set
      {
        this._listHeaderVisibility = value;
        this.NotifyPropertyChanged("ListHeaderVisibility");
      }
    }

    public List<SubscriptionItemHeader> FriendsImportProviders { get; private set; }

    public Func<VKList<User>, ListWithCount<SubscriptionItemHeader>> ConverterFunc
    {
      get
      {
        return (Func<VKList<User>, ListWithCount<SubscriptionItemHeader>>) (usersList =>
        {
          ListWithCount<SubscriptionItemHeader> listWithCount = new ListWithCount<SubscriptionItemHeader>();
          listWithCount.TotalCount = 1000;
          List<SubscriptionItemHeader> subscriptionItemHeaderList = new List<SubscriptionItemHeader>(usersList.items.Select<User, SubscriptionItemHeader>((Func<User, SubscriptionItemHeader>) (u => new SubscriptionItemHeader(u, true))));
          listWithCount.List = subscriptionItemHeaderList;
          return listWithCount;
        });
      }
    }

    public FriendsSearchViewModel(FriendsSearchMode mode)
    {
      this._mode = mode;
      this._searchVM = new GenericCollectionViewModel<VKList<User>, SubscriptionItemHeader>((ICollectionDataProvider<VKList<User>, SubscriptionItemHeader>) this);
      this.FriendsImportProviders = new List<SubscriptionItemHeader>()
      {
        new SubscriptionItemHeader(CommonResources.FindFriends_Gmail_Title, CommonResources.FindFriends_Gmail_Subtitle, "/Resources/FindFriends/FindFriendsGmail.png", "#ffef5350".GetColor(), (Action) (() => GmailFriendsImportProvider.Instance.Login())),
        new SubscriptionItemHeader(CommonResources.FindFriends_Facebook_Title, CommonResources.FindFriends_Facebook_Subtitle, "/Resources/FindFriends/FindFriendsFacebook.png", "#ff3f61ab".GetColor(), (Action) (() => FacebookFriendsImportProvider.Instance.Login())),
        new SubscriptionItemHeader(CommonResources.FindFriends_Twitter_Title, CommonResources.FindFriends_Twitter_Subtitle, "/Resources/FindFriends/FindFriendsTwitter.png", "#ff42a5f5".GetColor(), (Action) (() => TwitterFriendsImportProvider.Instance.Login())),
        new SubscriptionItemHeader(CommonResources.FindFriends_Nearby_Title, CommonResources.FindFriends_Nearby_Subtitle, "/Resources/FindFriends/FindFriendsNearby.png", "#ffb5b9bd".GetColor(), (Action) (() =>
        {
          if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked || !AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
          {
            bool flag = MessageBox.Show(CommonResources.MapAttachment_AllowUseLocation, CommonResources.AccessToLocation, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
            AppGlobalStateManager.Current.GlobalState.AllowUseLocationQuestionAsked = true;
            AppGlobalStateManager.Current.GlobalState.AllowUseLocation = flag;
          }
          if (!AppGlobalStateManager.Current.GlobalState.AllowUseLocation)
            return;
          Navigator.Current.NavigateToUsersSearchNearby();
        })),
        new SubscriptionItemHeader(CommonResources.FindFriends_Search_Title.ToLowerInvariant(), CommonResources.FindFriends_Search_Subtitle.ToLowerInvariant(), "/Resources/FindFriends/FindFriendsSearch.png", "#ff5888b7".GetColor(), (Action) (() => Navigator.Current.NavigateToUsersSearch("")))
      };
      if (this._mode != FriendsSearchMode.Default)
        return;
      this.FriendsImportProviders.Insert(0, new SubscriptionItemHeader(CommonResources.FindFriends_Contacts_Title, CommonResources.FindFriends_Contacts_Subtitle, "/Resources/FindFriends/FindFriendsContacts.png", "#ff42a5f5".GetColor(), (Action) (() => ContactsSyncRequestUC.OpenFriendsImportContacts((Action) (() => Navigator.Current.NavigateToFriendsImportContacts())))));
    }

    public void LoadData()
    {
      if (this._mode == FriendsSearchMode.Register)
        ContactsSyncRequestUC.OpenFriendsImportContacts((Action) (() =>
        {
          this._allowSendContacts = true;
          this._searchVM.LoadData(false, false, (Action<BackendResult<VKList<User>, ResultCode>>) null, false);
        }));
      else
        this._searchVM.LoadData(false, false, (Action<BackendResult<VKList<User>, ResultCode>>) null, false);
    }

    public void GetData(GenericCollectionViewModel<VKList<User>, SubscriptionItemHeader> caller, int offset, int count, Action<BackendResult<VKList<User>, ResultCode>> callback)
    {
      switch (this._mode)
      {
        case FriendsSearchMode.Default:
          UsersService.Instance.GetFriendsSuggestions(this._recommendationsNextFrom, count, (Action<BackendResult<VKList<User>, ResultCode>>) (e =>
          {
            if (e.ResultCode == ResultCode.Succeeded)
            {
              if (e.ResultData.items.Count > 0)
                Execute.ExecuteOnUIThread((Action) (() => this.ListHeaderVisibility = Visibility.Visible));
              this._recommendationsNextFrom = e.ResultData.next_from;
            }
            callback(e);
          }));
          break;
        case FriendsSearchMode.Register:
          if (this._contactsLoaded || !this._allowSendContacts)
            break;
          FriendsImportHelper.LoadData((IFriendsImportProvider) ContactsFriendsImportProvider.Instance, (Action<FriendsImportResponse>) (response =>
          {
            VKList<User> resultData = new VKList<User>();
            List<ISubscriptionItemHeader> foundUsers = response.FoundUsers;
            if (foundUsers.Count > 0)
            {
              Execute.ExecuteOnUIThread((Action) (() => this.ListHeaderVisibility = Visibility.Visible));
              List<User> list = foundUsers.OfType<SubscriptionItemHeader>().Select<SubscriptionItemHeader, User>((Func<SubscriptionItemHeader, User>) (subscriptionItem => subscriptionItem.GetUser())).Where<User>((Func<User, bool>) (user => user != null)).ToList<User>();
              resultData.count = list.Count;
              resultData.items.AddRange((IEnumerable<User>) list);
            }
            this._contactsLoaded = true;
            callback(new BackendResult<VKList<User>, ResultCode>(ResultCode.Succeeded, resultData));
          }), (Action<ResultCode>) (resultCode => callback(new BackendResult<VKList<User>, ResultCode>(resultCode, (VKList<User>) null))));
          break;
      }
    }

    public string GetFooterTextForCount(GenericCollectionViewModel<VKList<User>, SubscriptionItemHeader> caller, int count)
    {
      if (count <= 0)
        return "";
      return UIStringFormatterHelper.FormatNumberOfSomething(count, CommonResources.OnePersonFrm, CommonResources.TwoFourPersonsFrm, CommonResources.FivePersonsFrm, true, null, false);
    }
  }
}
