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
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class BannedUsersPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBarIconButton _appBarButtonDelete;
    private ApplicationBar _defaultAppBar;
    internal Grid LayoutRoot;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector listBoxBanned;
    private bool _contentLoaded;

    public BannedUsersViewModel BannedUsersVM
    {
      get
      {
        return base.DataContext as BannedUsersViewModel;
      }
    }

    public BannedUsersPage()
    {
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      string delete = CommonResources.Delete;
      applicationBarIconButton.Text = delete;
      Uri uri = new Uri("Resources/minus.png", UriKind.Relative);
      applicationBarIconButton.IconUri = uri;
      int num = 0;
      applicationBarIconButton.IsEnabled = (num != 0);
      this._appBarButtonDelete = applicationBarIconButton;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxBanned.ScrollToTop());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxBanned);
      this.listBoxBanned.OnRefresh = (Action) (() => this.BannedUsersVM.BannedVM.LoadData(true, false,  null, false));
      this.BuildAppBar();
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
      this._appBarButtonDelete.IsEnabled = (this.BannedUsersVM.SelectedCount > 0);
    }

    private void _appBarButtonDelete_Click(object sender, EventArgs e)
    {
      this.BannedUsersVM.DeleteSelected();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      BannedUsersViewModel bannedUsersViewModel = new BannedUsersViewModel();
      bannedUsersViewModel.BannedVM.LoadData(false, false,  null, false);
      base.DataContext = bannedUsersViewModel;
      bannedUsersViewModel.PropertyChanged += new PropertyChangedEventHandler(this.vm_PropertyChanged);
      this._isInitialized = true;
    }

    private void vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedCount"))
        return;
      this.UpdateAppBar();
    }

    private void ExtendedLongListSelector_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.BannedUsersVM.BannedVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/BannedUsersPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.listBoxBanned = (ExtendedLongListSelector) base.FindName("listBoxBanned");
    }
  }
}
