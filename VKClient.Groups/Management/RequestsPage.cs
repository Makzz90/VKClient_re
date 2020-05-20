using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.UC;
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
  public class RequestsPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC Header;
    internal ExtendedLongListSelector List;
    internal PullToRefreshUC PullToRefresh;
    private bool _contentLoaded;

    private RequestsViewModel ViewModel
    {
      get
      {
        return base.DataContext as RequestsViewModel;
      }
    }

    public RequestsPage()
    {
        this.InitializeComponent();
        this.Header.OnHeaderTap += (Action)(() => this.List.ScrollToTop());
        this.PullToRefresh.TrackListBox((ISupportPullToRefresh)this.List);
        this.List.OnRefresh = (Action)(() => this.ViewModel.Requests.LoadData(true, false, (Action<BackendResult<VKList<User>, ResultCode>>)null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      RequestsViewModel requestsViewModel = new RequestsViewModel(long.Parse(((Page) this).NavigationContext.QueryString["CommunityId"]));
      base.DataContext = requestsViewModel;
      requestsViewModel.Requests.LoadData(true, false,  null, false);
      this._isInitialized = true;
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.Requests.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = (ExtendedLongListSelector) sender;
      if (!(longListSelector.SelectedItem is FriendHeader))
        return;
      longListSelector.SelectedItem = null;
    }

    private void Request_OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FriendHeader dataContext = ((FrameworkElement) sender).DataContext as FriendHeader;
      if (dataContext == null)
        return;
      Navigator.Current.NavigateToUserProfile(dataContext.UserId, "", "", false);
    }

    private void Button_OnAcceptClicked(object sender, RoutedEventArgs e)
    {
      FriendHeader dataContext = ((FrameworkElement) sender).DataContext as FriendHeader;
      if (dataContext == null)
        return;
      this.ViewModel.HandleRequest(dataContext, true);
    }

    private void Button_OnDeclineClicked(object sender, RoutedEventArgs e)
    {
      FriendHeader dataContext = ((FrameworkElement) sender).DataContext as FriendHeader;
      if (dataContext == null)
        return;
      this.ViewModel.HandleRequest(dataContext, false);
    }

    private void Button_OnTapped(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    private void Separator_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/RequestsPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.List = (ExtendedLongListSelector) base.FindName("List");
      this.PullToRefresh = (PullToRefreshUC) base.FindName("PullToRefresh");
    }
  }
}
