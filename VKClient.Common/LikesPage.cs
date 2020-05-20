using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class LikesPage : PageBase
  {
    private bool _isInitialized;
    private const int _countToLoad = 30;
    private const int _countToReload = 100;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal ExtendedLongListSelector listBoxAll;
    internal PivotItem pivotItemShared;
    internal ExtendedLongListSelector listBoxShared;
    internal PivotItem pivotItemFriends;
    internal ExtendedLongListSelector listBoxFriends;
    private bool _contentLoaded;

    private LikesViewModel LikesVM
    {
      get
      {
        return base.DataContext as LikesViewModel;
      }
    }

    public LikesPage()
    {
      this.InitializeComponent();
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      long ownerId = long.Parse(((Page) this).NavigationContext.QueryString["OwnerId"]);
      long itemId = long.Parse(((Page) this).NavigationContext.QueryString["ItemId"]);
      int num1 = int.Parse(((Page) this).NavigationContext.QueryString["Type"]);
      int knownCount = int.Parse(((Page) this).NavigationContext.QueryString["knownCount"]);
      int num2 = bool.Parse(((Page) this).NavigationContext.QueryString["SelectFriendLikes"]) ? 1 : 0;
      LikesViewModel vm = new LikesViewModel(ownerId, itemId, (LikeObjectType) num1, knownCount);
      base.DataContext = vm;
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxAll);
      this.listBoxAll.OnRefresh = (Action) (() => vm.All.LoadData(true, false,  null, false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxShared);
      this.listBoxShared.OnRefresh = (Action) (() => vm.Shared.LoadData(true, false,  null, false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxFriends);
      this.listBoxFriends.OnRefresh = (Action) (() => vm.Friends.LoadData(true, false,  null, false));
      vm.All.LoadData(false, false,  null, false);
      if (num2 != 0)
        this.pivot.SelectedItem = this.pivotItemFriends;
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      FriendHeader selectedItem = longListSelector.SelectedItem as FriendHeader;
      if (selectedItem == null)
        return;
      if (selectedItem.IsGroupHeader)
        Navigator.Current.NavigateToGroup(selectedItem.GroupId, selectedItem.FullName, false);
      else
        Navigator.Current.NavigateToUserProfile(selectedItem.UserId, selectedItem.User.Name, "", false);
      longListSelector.SelectedItem = null;
    }

    private void All_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.LikesVM.All.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Shared_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.LikesVM.Shared.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Friends_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.LikesVM.Friends.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedItem == this.pivotItemFriends)
        this.LikesVM.Friends.LoadData(false, false,  null, false);
      if (this.pivot.SelectedItem != this.pivotItemShared)
        return;
      this.LikesVM.Shared.LoadData(false, false,  null, false);
    }

    private void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemAll && Enumerable.Any<FriendHeader>(this.LikesVM.All.Collection))
        this.listBoxAll.ScrollTo(Enumerable.First<FriendHeader>(this.LikesVM.All.Collection));
      else if (this.pivot.SelectedItem == this.pivotItemFriends && Enumerable.Any<FriendHeader>(this.LikesVM.Friends.Collection))
      {
        this.listBoxFriends.ScrollTo(Enumerable.First<FriendHeader>(this.LikesVM.Friends.Collection));
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemShared || !Enumerable.Any<FriendHeader>(this.LikesVM.Shared.Collection))
          return;
        this.listBoxShared.ScrollTo(Enumerable.First<FriendHeader>(this.LikesVM.Shared.Collection));
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/LikesPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemAll = (PivotItem) base.FindName("pivotItemAll");
      this.listBoxAll = (ExtendedLongListSelector) base.FindName("listBoxAll");
      this.pivotItemShared = (PivotItem) base.FindName("pivotItemShared");
      this.listBoxShared = (ExtendedLongListSelector) base.FindName("listBoxShared");
      this.pivotItemFriends = (PivotItem) base.FindName("pivotItemFriends");
      this.listBoxFriends = (ExtendedLongListSelector) base.FindName("listBoxFriends");
    }
  }
}
