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
using VKClient.Groups.Management.Library;

namespace VKClient.Groups.Management
{
  public class InvitationsPage : PageBase
  {
    private bool _isInitialized;
    internal GenericHeaderUC Header;
    internal ExtendedLongListSelector List;
    internal PullToRefreshUC PullToRefresh;
    private bool _contentLoaded;

    private InvitationsViewModel ViewModel
    {
      get
      {
        return base.DataContext as InvitationsViewModel;
      }
    }

    public InvitationsPage()
    {
        this.InitializeComponent();
        this.Header.OnHeaderTap += (Action)(() => this.List.ScrollToTop());
        this.PullToRefresh.TrackListBox((ISupportPullToRefresh)this.List);
        this.List.OnRefresh = (Action)(() => this.ViewModel.Invitations.LoadData(true, false, (Action<BackendResult<VKList<User>, ResultCode>>)null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      InvitationsViewModel invitationsViewModel = new InvitationsViewModel(long.Parse(((Page) this).NavigationContext.QueryString["CommunityId"]));
      base.DataContext = invitationsViewModel;
      invitationsViewModel.Invitations.LoadData(true, false,  null, false);
      this._isInitialized = true;
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      this.ViewModel.Invitations.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = (ExtendedLongListSelector) sender;
      LinkHeader selectedItem = longListSelector.SelectedItem as LinkHeader;
      if (selectedItem == null)
        return;
      longListSelector.SelectedItem = null;
      Navigator.Current.NavigateToUserProfile(selectedItem.Id, "", "", false);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/InvitationsPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.List = (ExtendedLongListSelector) base.FindName("List");
      this.PullToRefresh = (PullToRefreshUC) base.FindName("PullToRefresh");
    }
  }
}
