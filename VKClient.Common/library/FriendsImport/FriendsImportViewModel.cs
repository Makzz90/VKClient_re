using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        if (!this._isLoading)
          return Visibility.Collapsed;
        return Visibility.Visible;
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
          Func<Group<ISubscriptionItemHeader>, bool> func1 = (Func<Group<ISubscriptionItemHeader>, bool>) (items => Enumerable.Any<ISubscriptionItemHeader>(items));
          if (!Enumerable.Any<Group<ISubscriptionItemHeader>>(collection, func1))
            return CommonResources.FriendsImport_NoContactsFound;
        }
        return "";
      }
    }

    public Visibility StatusTextVisibility
    {
      get
      {
        if (string.IsNullOrEmpty(this.StatusText))
          return Visibility.Collapsed;
        return Visibility.Visible;
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
      this._friendsImportProvider.CompleteLogin((Action<bool>)(succeeded =>
      {
          if (!succeeded)
          {
              this._isError = true;
              this._isLoading = false;
              this._isLoaded = true;
              this.NotifyProperties();
          }
          else
              FriendsImportHelper.LoadData(this._friendsImportProvider, delegate(FriendsImportResponse response)
                    {
                        Execute.ExecuteOnUIThread(delegate
                        {
                            List<ISubscriptionItemHeader> foundUsers = response.FoundUsers;
                            List<ISubscriptionItemHeader> otherUsers = response.OtherUsers;
                            if (foundUsers.Count > 0)
                            {
                                Group<ISubscriptionItemHeader> group = new Group<ISubscriptionItemHeader>("", false);
                                using (List<ISubscriptionItemHeader>.Enumerator enumerator = foundUsers.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        ISubscriptionItemHeader current = enumerator.Current;
                                        group.Add(current);
                                    }
                                }
                                this.Collection.Add(group);
                            }
                            if (otherUsers.Count > 0)
                            {
                                Group<ISubscriptionItemHeader> group2 = new Group<ISubscriptionItemHeader>(CommonResources.Title_InviteToVK, false);
                                using (List<ISubscriptionItemHeader>.Enumerator enumerator = otherUsers.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        ISubscriptionItemHeader current2 = enumerator.Current;
                                        group2.Add(current2);
                                    }
                                }
                                this.Collection.Add(group2);
                            }
                            this._isLoading = false;
                            this._isLoaded = true;
                            this.NotifyProperties();
                        });
                    }, delegate(ResultCode resultCode)
                    {
                        this._isError = true;
                        this._isLoading = false;
                        this._isLoaded = true;
                        this.NotifyProperties();
                    });
      }));
    }

    private void NotifyProperties()
    {
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<bool>(() => this.IsLoading);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.IsLoadingVisibility);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<string>(() => this.StatusText);
      // ISSUE: type reference
      // ISSUE: method reference
      this.NotifyPropertyChanged<Visibility>(() => this.StatusTextVisibility);
    }
  }
}
