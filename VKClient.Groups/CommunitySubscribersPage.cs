using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;
using VKClient.Groups.Library;

using VKClient.Audio.Base.Extensions;
namespace VKClient.Groups
{
  public class CommunitySubscribersPage : PageBase
  {
    private bool _isInitialized;
    private bool _isManagement;
    private bool _isPicker;
    private bool _isBlockingPicker;
    private long _communityId;
    internal GenericHeaderUC Header;
    internal Pivot Pivot;
    internal PivotItem PivotItemAll;
    internal ExtendedLongListSelector AllList;
    internal PivotItem PivotItemUnsure;
    internal ExtendedLongListSelector UnsureList;
    internal PivotItem PivotItemFriends;
    internal ExtendedLongListSelector FriendsList;
    internal PullToRefreshUC PullToRefresh;
    private bool _contentLoaded;

    public CommunitySubscribersViewModel ViewModel
    {
      get
      {
        return base.DataContext as CommunitySubscribersViewModel;
      }
    }

    public CommunitySubscribersPage()
    {
      this.InitializeComponent();
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.AllList);
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.UnsureList);
      this.PullToRefresh.TrackListBox((ISupportPullToRefresh) this.FriendsList);
      this.Header.OnHeaderTap = (Action) (() =>
      {
        if (this.Pivot.SelectedItem == this.PivotItemAll)
          this.AllList.ScrollToTop();
        if (this.Pivot.SelectedItem == this.PivotItemUnsure)
          this.UnsureList.ScrollToTop();
        if (this.Pivot.SelectedItem != this.PivotItemFriends)
          return;
        this.FriendsList.ScrollToTop();
      });
      this.AllList.OnRefresh = (Action) (() => this.ViewModel.All.LoadData(true, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false));
      this.UnsureList.OnRefresh = (Action) (() => this.ViewModel.Unsure.LoadData(true, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false));
      this.FriendsList.OnRefresh = (Action) (() => this.ViewModel.Friends.LoadData(true, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      GroupType toEnum = ((Page) this).NavigationContext.QueryString["CommunityType"].ParseToEnum<GroupType>();
      this._communityId = long.Parse(((Page) this).NavigationContext.QueryString["CommunityId"]);
      this._isManagement = ((Page) this).NavigationContext.QueryString["IsManagement"].ToLower() == "true";
      this._isPicker = ((Page) this).NavigationContext.QueryString["IsPicker"].ToLower() == "true";
      this._isBlockingPicker = ((Page) this).NavigationContext.QueryString["IsBlockingPicker"].ToLower() == "true";
      if (toEnum != GroupType.Event)
        ((PresentationFrameworkCollection<object>) ((ItemsControl) this.Pivot).Items).Remove((object) this.PivotItemUnsure);
      CommunitySubscribersViewModel subscribersViewModel = new CommunitySubscribersViewModel(this._communityId, toEnum, this._isManagement);
      base.DataContext=((object) subscribersViewModel);
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri=(uri);
      string pageAppBarSearch = CommonResources.FriendsPage_AppBar_Search;
      applicationBarIconButton1.Text=(pageAppBarSearch);
      ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
      applicationBarIconButton2.Click+=(new EventHandler(this.SearchButton_OnClicked));
      this.ApplicationBar=((IApplicationBar) ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9));
      this.ApplicationBar.Buttons.Add((object) applicationBarIconButton2);
      if (this._isPicker)
      {
        this.Header.HideSandwitchButton = true;
        this.SuppressMenu = true;
      }
      subscribersViewModel.All.LoadData(false, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false);
      this._isInitialized = true;
    }

    private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.Pivot.SelectedItem == this.PivotItemUnsure)
        this.ViewModel.Unsure.LoadData(false, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false);
      if (this.Pivot.SelectedItem != this.PivotItemFriends)
        return;
      this.ViewModel.Friends.LoadData(false, false, (Action<BackendResult<CommunitySubscribers, ResultCode>>) null, false);
    }

    private void List_OnLinked(object sender, LinkUnlinkEventArgs e)
    {
      if (sender == this.AllList)
        this.ViewModel.All.LoadMoreIfNeeded(e.ContentPresenter.Content);
      if (sender == this.UnsureList)
        this.ViewModel.Unsure.LoadMoreIfNeeded(e.ContentPresenter.Content);
      if (sender != this.FriendsList)
        return;
      this.ViewModel.Friends.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void List_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = (ExtendedLongListSelector) sender;
      LinkHeader item = longListSelector.SelectedItem as LinkHeader;
      if (item == null)
        return;
      longListSelector.SelectedItem=((object) null);
      if (!this._isPicker)
        Navigator.Current.NavigateToUserProfile(item.Id, item.User.Name, "", false);
      else if (item.Id != AppGlobalStateManager.Current.LoggedInUserId && (this.ViewModel.Managers == null || this.ViewModel.Managers.All<User>((Func<User, bool>) (m => m.id != item.Id))))
      {
        if (!this._isBlockingPicker)
          Navigator.Current.NavigateToCommunityManagementManagerAdding(this.ViewModel.CommunityId, this.ViewModel.CommunityType, item.User, true);
        else
          Navigator.Current.NavigateToCommunityManagementBlockAdding(this.ViewModel.CommunityId, item.User, false);
      }
      else
        new GenericInfoUC().ShowAndHideLater(CommonResources.Error, (FrameworkElement) null);
    }

    private void AddToManagers_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader dataContext = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (dataContext == null)
        return;
      Navigator.Current.NavigateToCommunityManagementManagerAdding(this.ViewModel.CommunityId, this.ViewModel.CommunityType, dataContext.User, false);
    }

    private void Edit_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader dataContext = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (dataContext == null)
        return;
      this.ViewModel.NavigateToManagerEditing(dataContext);
    }

    private void RemoveFromCommunity_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader dataContext = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (dataContext == null || MessageBox.Show(CommonResources.GenericConfirmation, CommonResources.RemovingFromCommunity, (MessageBoxButton) 1) != MessageBoxResult.OK)
        return;
      this.ViewModel.RemoveFromCommunity(dataContext);
    }

    private void Block_OnClicked(object sender, RoutedEventArgs e)
    {
      LinkHeader dataContext = ((FrameworkElement) sender).DataContext as LinkHeader;
      if (dataContext == null)
        return;
      Navigator.Current.NavigateToCommunityManagementBlockAdding(this._communityId, dataContext.User, true);
    }

    private void SearchButton_OnClicked(object sender, EventArgs e)
    {
        if (this.ViewModel.Managers == null)
        {
            return;
        }
        DialogService expr_20 = new DialogService();
        expr_20.BackgroundBrush = new SolidColorBrush(Colors.Transparent);
        expr_20.AnimationType = DialogService.AnimationTypes.None;
        expr_20.HideOnNavigation = false;
        DataTemplate itemTemplate = (DataTemplate)base.Resources["ItemTemplate"];
        CommunitySubscribersSearchDataProvider searchDataProvider = new CommunitySubscribersSearchDataProvider(this._communityId, this.ViewModel.CommunityType, this.ViewModel.Managers, this._isManagement, this.Pivot.SelectedItem == this.PivotItemFriends);
        GenericSearchUC searchUC = new GenericSearchUC();
        searchUC.LayoutRootGrid.Margin=(new Thickness(0.0, 77.0, 0.0, 0.0));
        searchUC.Initialize<User, LinkHeader>(searchDataProvider, delegate(object p, object f)
        {
            this.List_OnSelectionChanged(p, null);
        }, itemTemplate);
        searchUC.SearchTextBox.TextChanged+=(delegate(object s, TextChangedEventArgs ev)
        {
            bool flag = searchUC.SearchTextBox.Text != "";
            this.Pivot.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
        });
        expr_20.Closed += delegate(object p, EventArgs f)
        {
            this.Pivot.Visibility=(0);
            this.ViewModel.SearchViewModel = null;
        };
        expr_20.Child = searchUC;
        expr_20.Show(this.Pivot);
        this.InitializeAdornerControls();
        this.ViewModel.SearchViewModel = ((GenericSearchViewModel<User, LinkHeader>)searchUC.ViewModel).SearchVM;
    }


    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Groups;component/CommunitySubscribersPage.xaml", UriKind.Relative));
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.Pivot = (Pivot) base.FindName("Pivot");
      this.PivotItemAll = (PivotItem) base.FindName("PivotItemAll");
      this.AllList = (ExtendedLongListSelector) base.FindName("AllList");
      this.PivotItemUnsure = (PivotItem) base.FindName("PivotItemUnsure");
      this.UnsureList = (ExtendedLongListSelector) base.FindName("UnsureList");
      this.PivotItemFriends = (PivotItem) base.FindName("PivotItemFriends");
      this.FriendsList = (ExtendedLongListSelector) base.FindName("FriendsList");
      this.PullToRefresh = (PullToRefreshUC) base.FindName("PullToRefresh");
    }
  }
}
