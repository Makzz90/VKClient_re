using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FriendRequestsPage : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal ExtendedLongListSelector listBoxFriendRequests;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private FriendRequestsViewModel FriendRequestsVM
    {
      get
      {
        return base.DataContext as FriendRequestsViewModel;
      }
    }

    public FriendRequestsPage()
    {
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxFriendRequests.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxFriendRequests);
      this.listBoxFriendRequests.OnRefresh = (Action) (() => this.FriendRequestsVM.FriendRequestsVM.LoadData(true, false,  null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      FriendRequestsViewModel requestsViewModel = new FriendRequestsViewModel(((Page) this).NavigationContext.QueryString.ContainsKey("AreSuggestedFriends") && bool.Parse(((Page) this).NavigationContext.QueryString["AreSuggestedFriends"]));
      requestsViewModel.ParentFriendRequestsUC = (FriendRequestsUC) ParametersRepository.GetParameterForIdAndReset("FriendRequestsUC");
      base.DataContext = requestsViewModel;
      requestsViewModel.FriendRequestsVM.LoadData(false, false,  null, false);
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      if (!(this.listBoxFriendRequests.SelectedItem is FriendHeader))
        return;
      this.listBoxFriendRequests.SelectedItem = null;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.FriendRequestsVM.FriendRequestsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FriendRequestsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.listBoxFriendRequests = (ExtendedLongListSelector) base.FindName("listBoxFriendRequests");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
