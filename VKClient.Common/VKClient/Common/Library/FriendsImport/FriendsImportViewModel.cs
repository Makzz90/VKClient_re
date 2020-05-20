using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Localization;

namespace VKClient.Common.Library.FriendsImport
{
  public class FriendsImportViewModel : ViewModelBase
  {
    private bool _isLoading;
    private bool _isError;
    private bool _isLoaded;
    private readonly IFriendsImportProvider _friendsImportProvider;

    public ObservableCollection<Group<ISubscriptionItemHeader>> Collection { get; private set; }

    public bool IsLoading
    {
      get
      {
        return this._isLoading;
      }
    }

    public Visibility IsLoadingVisibility
    {
      get
      {
        return !this._isLoading ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public string StatusText
    {
      get
      {
        if (this._isLoading)
          return CommonResources.Loading;
        if (this._isError)
          return CommonResources.Error_Generic;
        if (this._isLoaded)
        {
          ObservableCollection<Group<ISubscriptionItemHeader>> collection = this.Collection;
          Func<Group<ISubscriptionItemHeader>, bool> predicate = (Func<Group<ISubscriptionItemHeader>, bool>)(items => items.Any<ISubscriptionItemHeader>());
          //Func<Group<ISubscriptionItemHeader>, bool> predicate = null;
          if (!collection.Any<Group<ISubscriptionItemHeader>>(predicate))
            return CommonResources.FriendsImport_NoContactsFound;
        }
        return "";
      }
    }

    public Visibility StatusTextVisibility
    {
      get
      {
        return string.IsNullOrEmpty(this.StatusText) ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public FriendsImportViewModel(IFriendsImportProvider friendsImportProvider)
    {
      this._friendsImportProvider = friendsImportProvider;
      this.Collection = new ObservableCollection<Group<ISubscriptionItemHeader>>();
    }

    public void LoadData()
    {
      this._isLoaded = false;
      this._isError = false;
      this._isLoading = true;
      this.NotifyProperties();
      this._friendsImportProvider.CompleteLogin((Action<bool>) (succeeded =>
      {
        if (!succeeded)
        {
          this._isError = true;
          this._isLoading = false;
          this._isLoaded = true;
          this.NotifyProperties();
        }
        else
          FriendsImportHelper.LoadData(this._friendsImportProvider, (Action<FriendsImportResponse>) (response => Execute.ExecuteOnUIThread((Action) (() =>
          {
            List<ISubscriptionItemHeader> foundUsers = response.FoundUsers;
            List<ISubscriptionItemHeader> otherUsers = response.OtherUsers;
            if (foundUsers.Count > 0)
            {
              Group<ISubscriptionItemHeader> group = new Group<ISubscriptionItemHeader>("", false);
              foreach (ISubscriptionItemHeader subscriptionItemHeader in foundUsers)
                group.Add(subscriptionItemHeader);
              this.Collection.Add(group);
            }
            if (otherUsers.Count > 0)
            {
              Group<ISubscriptionItemHeader> group = new Group<ISubscriptionItemHeader>(CommonResources.Title_InviteToVK, false);
              foreach (ISubscriptionItemHeader subscriptionItemHeader in otherUsers)
                group.Add(subscriptionItemHeader);
              this.Collection.Add(group);
            }
            this._isLoading = false;
            this._isLoaded = true;
            this.NotifyProperties();
          }))), (Action<ResultCode>) (resultCode =>
          {
            this._isError = true;
            this._isLoading = false;
            this._isLoaded = true;
            this.NotifyProperties();
          }));
      }));
    }

    private void NotifyProperties()
    {
      this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsLoading));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.IsLoadingVisibility));
      this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.StatusText));
      this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.StatusTextVisibility));
    }
  }
}
