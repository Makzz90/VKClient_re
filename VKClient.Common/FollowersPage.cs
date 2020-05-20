using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FollowersPage : PageBase
  {
    private bool _isInitialized;
    private bool _subscriptions;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBox;
    private bool _contentLoaded;

    private FollowersViewModel FollowersVM
    {
      get
      {
        return base.DataContext as FollowersViewModel;
      }
    }

    public FollowersPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBox.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBox);
      this.listBox.OnRefresh = (Action) (() =>
      {
        if (this._subscriptions)
          this.FollowersVM.SubscriptionsVM.LoadData(true, false,  null, false);
        else
          this.FollowersVM.FollowersVM.LoadData(true, false,  null, false);
      });
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      string name = "";
      if (((Page) this).NavigationContext.QueryString.ContainsKey("Name"))
        name = ((Page) this).NavigationContext.QueryString["Name"];
      if (((Page) this).NavigationContext.QueryString.ContainsKey("Mode") && ((Page) this).NavigationContext.QueryString["Mode"] == "Subscriptions")
      {
        this._subscriptions = true;
        ((FrameworkElement) this.listBox).SetBinding((DependencyProperty) FrameworkElement.DataContextProperty, new Binding("SubscriptionsVM"));
      }
      FollowersViewModel followersViewModel = new FollowersViewModel(this.CommonParameters.UserOrGroupId, this.CommonParameters.IsGroup, name, this._subscriptions);
      base.DataContext = followersViewModel;
      if (!this._subscriptions)
        followersViewModel.FollowersVM.LoadData(false, false,  null, false);
      else
        followersViewModel.SubscriptionsVM.LoadData(false, false,  null, false);
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      if (!this._subscriptions)
        this.FollowersVM.FollowersVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
      else
        this.FollowersVM.SubscriptionsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      UserGroupHeader selectedItem = this.listBox.SelectedItem as UserGroupHeader;
      if (selectedItem == null)
        return;
      if (selectedItem.UserHeader != null)
        Navigator.Current.NavigateToUserProfile(selectedItem.UserHeader.UserId, selectedItem.UserHeader.User.Name, "", false);
      else if (selectedItem.GroupHeader != null)
        Navigator.Current.NavigateToGroup(selectedItem.GroupHeader.Group.id, selectedItem.GroupHeader.Group.name, false);
      this.listBox.SelectedItem = null;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FollowersPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBox = (ExtendedLongListSelector) base.FindName("listBox");
    }
  }
}
