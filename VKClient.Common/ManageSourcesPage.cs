using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Common
{
  public class ManageSourcesPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBarIconButton _appBarButtonDelete;
    private ApplicationBar _defaultAppBar;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal Pivot pivot;
    internal PivotItem pivotItemFriends;
    internal ExtendedLongListSelector listBoxFriends;
    internal PivotItem pivotItemCommunities;
    internal ExtendedLongListSelector listBoxCommunities;
    private bool _contentLoaded;

    private ManageSourcesViewModel ManageSourcesVM
    {
      get
      {
        return base.DataContext as ManageSourcesViewModel;
      }
    }

    public ManageSourcesPage()
    {
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      string delete = CommonResources.Delete;
      applicationBarIconButton.Text = delete;
      Uri uri = new Uri("Resources/minus.png", UriKind.Relative);
      applicationBarIconButton.IconUri = uri;
      this._appBarButtonDelete = applicationBarIconButton;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() =>
      {
        if (this.pivot.SelectedItem == this.pivotItemFriends)
        {
          this.listBoxFriends.ScrollToTop();
        }
        else
        {
          if (this.pivot.SelectedItem != this.pivotItemCommunities)
            return;
          this.listBoxCommunities.ScrollToTop();
        }
      });
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxCommunities);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxFriends);
      this.listBoxFriends.OnRefresh = (Action) (() => this.ManageSourcesVM.FriendsVM.LoadData(true, false,  null, false));
      this.listBoxCommunities.OnRefresh = (Action) (() => this.ManageSourcesVM.GroupsVM.LoadData(true, false,  null, false));
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._defaultAppBar = applicationBar;
      this._appBarButtonDelete.Click+=(new EventHandler(this._appBarButtonDelete_Click));
      this._defaultAppBar.Buttons.Add(this._appBarButtonDelete);
      this.ApplicationBar = ((IApplicationBar) this._defaultAppBar);
    }

    private void UpdateAppBar()
    {
      this._appBarButtonDelete.IsEnabled = (this.ManageSourcesVM.SelectedCount > 0);
    }

    private void _appBarButtonDelete_Click(object sender, EventArgs e)
    {
      this.ManageSourcesVM.DeleteSelected();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        ManageSourcesViewModel sourcesViewModel = new ManageSourcesViewModel((ManageSourcesMode) Enum.Parse(typeof (ManageSourcesMode), ((Page) this).NavigationContext.QueryString["Mode"]));
        base.DataContext = sourcesViewModel;
        sourcesViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
        sourcesViewModel.FriendsVM.LoadData(false, false,  null, false);
        sourcesViewModel.GroupsVM.LoadData(false, false,  null, false);
        this.BuildAppBar();
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedCount"))
        return;
      this.UpdateAppBar();
    }

    private void ExtendedLongListSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      (sender as ExtendedLongListSelector).SelectedItem = null;
    }

    private void Friend_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!(sender is FrameworkElement))
        return;
      FriendHeader dataContext = (sender as FrameworkElement).DataContext as FriendHeader;
      if (dataContext == null)
        return;
      Navigator.Current.NavigateToUserProfile(dataContext.UserId, dataContext.User.Name, "", false);
    }

    private void Group_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (!(sender is FrameworkElement))
        return;
      GroupHeader dataContext = (sender as FrameworkElement).DataContext as GroupHeader;
      if (dataContext == null)
        return;
      Navigator.Current.NavigateToGroup(dataContext.Group.id, dataContext.Group.name, false);
    }

    private void CheckBox_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/ManageSourcesPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemFriends = (PivotItem) base.FindName("pivotItemFriends");
      this.listBoxFriends = (ExtendedLongListSelector) base.FindName("listBoxFriends");
      this.pivotItemCommunities = (PivotItem) base.FindName("pivotItemCommunities");
      this.listBoxCommunities = (ExtendedLongListSelector) base.FindName("listBoxCommunities");
    }
  }
}
