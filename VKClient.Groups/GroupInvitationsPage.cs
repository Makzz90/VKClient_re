using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.UC;
using VKClient.Groups.Library;
using VKClient.Groups.Localization;
using VKClient.Groups.UC;

namespace VKClient.Groups
{
  public class GroupInvitationsPage : PageBase
  {
    private bool _isInitialized;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector listBoxRequests;
    private bool _contentLoaded;

    private GroupInvitationsViewModel GroupInvitationsVM
    {
      get
      {
        return base.DataContext as GroupInvitationsViewModel;
      }
    }

    public GroupInvitationsPage()
    {
        this.InitializeComponent();
        this.ucHeader.TextBlockTitle.Text=(GroupResources.GroupInvitationsPage_Title);
        this.ucHeader.OnHeaderTap = (Action)(() => this.listBoxRequests.ScrollToTop());
        this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh)this.listBoxRequests);
        this.listBoxRequests.OnRefresh = (Action)(() => this.GroupInvitationsVM.InvitationsVM.LoadData(true, false, null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      GroupInvitationsViewModel invitationsViewModel = new GroupInvitationsViewModel();
      invitationsViewModel.ParentCommunityInvitationsUC = (CommunityInvitationsUC) ParametersRepository.GetParameterForIdAndReset("CommunityInvitationsUC");
      base.DataContext = invitationsViewModel;
      invitationsViewModel.LoadInvitations();
      this._isInitialized = true;
    }

    private void ExtendedLongListSelector_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.GroupInvitationsVM.InvitationsVM.LoadMoreIfNeeded((e.ContentPresenter.Content as GroupHeader));
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/GroupInvitationsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.listBoxRequests = (ExtendedLongListSelector) base.FindName("listBoxRequests");
    }
  }
}
