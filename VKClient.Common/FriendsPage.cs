using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common
{
  public class FriendsPage : PageBase
  {
    private bool _isInitialized;
    private NewFriendsListUC _createFriendsListUC;
    private DialogService _dialogService;
    private FriendsPageMode _mode;
    private readonly ApplicationBarIconButton _appBarButtonCreateList;
    private readonly ApplicationBarIconButton _appBarButtonAddToList;
    private readonly ApplicationBarIconButton _appBarButtonSearch;
    private readonly ApplicationBarIconButton _appBarButtonAdd;
    private ApplicationBar _friendsListAppBar;
    private ApplicationBar _mainAppBar;
    private bool _mutualNavigationPerformed;
    private bool _loadedLists;
    private bool _loadedOnline;
    private bool _loadedCommon;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal ExtendedLongListSelector allFriendsListBox;
    internal PivotItem pivotItemOnline;
    internal ExtendedLongListSelector onlineFriendsListBox;
    internal PivotItem pivotItemLists;
    internal ExtendedLongListSelector friendListsListBox;
    internal PivotItem pivotItemMutualFriends;
    internal ExtendedLongListSelector mutualFriendsListBox;
    private bool _contentLoaded;

    private FriendsViewModel FriendsVM
    {
      get
      {
        return base.DataContext as FriendsViewModel;
      }
    }

    public FriendsPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string friendsPageCreateList = CommonResources.FriendsPage_CreateList;
      applicationBarIconButton1.Text = friendsPageCreateList;
      this._appBarButtonCreateList = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string pageAppBarAddToList = CommonResources.FriendsPage_AppBar_AddToList;
      applicationBarIconButton2.Text = pageAppBarAddToList;
      this._appBarButtonAddToList = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton3.IconUri = uri3;
      string pageAppBarSearch = CommonResources.FriendsPage_AppBar_Search;
      applicationBarIconButton3.Text = pageAppBarSearch;
      this._appBarButtonSearch = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton4.IconUri = uri4;
      string friendsPageAppBarAdd = CommonResources.FriendsPage_AppBar_Add;
      applicationBarIconButton4.Text = friendsPageAppBarAdd;
      this._appBarButtonAdd = applicationBarIconButton4;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      // ISSUE: method pointer
      base.Loaded+=(new RoutedEventHandler( this.FriendsPage_Loaded));
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    private void BuildAppBar(bool isCurrentUser)
    {
      this._friendsListAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._mainAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._appBarButtonCreateList.Click+=(new EventHandler(this._appBarButtonCreateList_Click));
      this._appBarButtonAddToList.Click+=(new EventHandler(this._appBarButtonAddToList_Click));
      this._appBarButtonSearch.Click+=(new EventHandler(this._appBarButtonSearch_Click));
      this._mainAppBar.Buttons.Add(this._appBarButtonSearch);
      if (isCurrentUser)
      {
        this._appBarButtonAdd.Click+=(new EventHandler(this._appBarButtonAdd_Click));
        this._mainAppBar.Buttons.Add(this._appBarButtonAdd);
      }
      this._friendsListAppBar.Buttons.Add(this._appBarButtonCreateList);
    }

    private void UpdateAppBar()
    {
      if (this.pivot.SelectedItem == this.pivotItemLists)
      {
        this.ApplicationBar = ( null);
      }
      else
      {
        if (this.pivot.SelectedItem == this.pivotItemMutualFriends)
          return;
        if (this.FriendsVM.FriendsMode == FriendsViewModel.Mode.Lists)
          this._mainAppBar.Buttons.Contains(this._appBarButtonAddToList);
        this.ApplicationBar = ((IApplicationBar) this._mainAppBar);
      }
    }

    private void _appBarButtonAddToList_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToFriends(AppGlobalStateManager.Current.LoggedInUserId, "", false, FriendsPageMode.PickAndBack);
    }

    private void _appBarButtonCreateList_Click(object sender, EventArgs e)
    {
      this._dialogService = new DialogService();
      this._dialogService.SetStatusBarBackground = true;
      this._createFriendsListUC = new NewFriendsListUC();
      this._createFriendsListUC.Initialize(true);
      this._dialogService.Child = (FrameworkElement) this._createFriendsListUC;
      // ISSUE: method pointer
      ((ButtonBase) this._createFriendsListUC.buttonCreate).Click+=(new RoutedEventHandler( this.buttonCreate_Click));
      this._dialogService.Show( null);
    }

    private void _appBarButtonSearch_Click(object sender, EventArgs e)
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
//      FriendsPage.<>c__DisplayClass17_0 cDisplayClass170 = new FriendsPage.<>c__DisplayClass17_0();
      // ISSUE: reference to a compiler-generated field
 //     cDisplayClass170.<>4__this = this;
      DialogService dialogService = new DialogService();
      dialogService.BackgroundBrush = (Brush) new SolidColorBrush(Colors.Transparent);
      dialogService.AnimationType = DialogService.AnimationTypes.None;
      int num = 0;
      dialogService.HideOnNavigation = num != 0;
      this._dialogService = dialogService;
      UsersSearchDataProvider searchDataProvider = new UsersSearchDataProvider((IEnumerable<FriendHeader>) Enumerable.Select<User, FriendHeader>(this.FriendsVM.AllFriendsRaw, (Func<User, FriendHeader>) (f => new FriendHeader(f, false))), this._mode == FriendsPageMode.Default);
      DataTemplate itemTemplate = (DataTemplate) Application.Current.Resources["FriendItemTemplate"];
      // ISSUE: reference to a compiler-generated field
      GenericSearchUC searchUC = new GenericSearchUC();
      // ISSUE: reference to a compiler-generated field
      ((FrameworkElement) searchUC.LayoutRootGrid).Margin=(new Thickness(0.0, 77.0, 0.0, 0.0));
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      searchUC.Initialize<User, FriendHeader>((ISearchDataProvider<User, FriendHeader>) searchDataProvider, new Action<object, object>( this.HandleSearchSelectionChanged), itemTemplate);
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      searchUC.SearchTextBox.TextChanged += (delegate(object s, TextChangedEventArgs ev)
      {
          bool flag = searchUC.SearchTextBox.Text != string.Empty;
          this.pivot.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
      });
      // ISSUE: reference to a compiler-generated field
      this._dialogService.Child = (FrameworkElement) searchUC;
      this._dialogService.Show((UIElement) this.pivot);
    }

    private void HandleSearchSelectionChanged(object listBox, object selectedItem)
    {
      FriendHeader friendHeader = selectedItem as FriendHeader;
      if (friendHeader == null)
        return;
      Navigator.Current.NavigateToUserProfile(friendHeader.UserId, friendHeader.User.Name, "", false);
    }

    private void _appBarButtonAdd_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToFriendsSuggestions();
    }

    private void buttonCreate_Click(object sender, RoutedEventArgs e)
    {
    }

    private void FriendsPage_Loaded(object sender, RoutedEventArgs e)
    {
      if (!((Page) this).NavigationContext.QueryString.ContainsKey("Mutual") || !(((Page) this).NavigationContext.QueryString["Mutual"] == bool.TrueString) || this._mutualNavigationPerformed)
        return;
      this.pivot.SelectedItem = this.pivotItemMutualFriends;
      this._mutualNavigationPerformed = true;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      bool isCurrentUser = false;
      FriendsViewModel vm;
      if (((Page) this).NavigationContext.QueryString.ContainsKey("ListId"))
      {
        vm = new FriendsViewModel(long.Parse(((Page) this).NavigationContext.QueryString["ListId"]), ((Page) this).NavigationContext.QueryString["ListName"], true);
      }
      else
      {
        long userId = this.CommonParameters.UserId;
        string name = "";
        if (((Page) this).NavigationContext.QueryString.ContainsKey("Name"))
          name = ((Page) this).NavigationContext.QueryString["Name"];
        vm = new FriendsViewModel(userId, name);
        isCurrentUser = userId == AppGlobalStateManager.Current.LoggedInUserId;
      }
      this.BuildAppBar(isCurrentUser);
      if (((Page) this).NavigationContext.QueryString.ContainsKey("Mode"))
      {
        // ISSUE: type reference
        this._mode = (FriendsPageMode) Enum.Parse(typeof (FriendsPageMode), base.NavigationContext.QueryString["Mode"], true);
      }
      base.DataContext = vm;
      vm.LoadFriends();
      if (vm.FriendsMode == FriendsViewModel.Mode.Friends)
      {
        if (this._mode == FriendsPageMode.Default)
        {
          if (vm.OwnFriends)
            ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemMutualFriends);
          else
            ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemLists);
        }
        if (this._mode == FriendsPageMode.PickAndBack)
        {
          ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemLists);
          ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemMutualFriends);
        }
      }
      if (vm.FriendsMode == FriendsViewModel.Mode.Lists)
      {
        ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemLists);
        ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemMutualFriends);
      }
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.allFriendsListBox);
      this.allFriendsListBox.OnRefresh = (Action) (() => vm.RefreshFriends(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.onlineFriendsListBox);
      this.onlineFriendsListBox.OnRefresh = (Action) (() => vm.RefreshFriends(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.mutualFriendsListBox);
      this.mutualFriendsListBox.OnRefresh = (Action) (() => vm.RefreshFriends(false));
      this._isInitialized = true;
    }

    private void friendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selectedItem = this.allFriendsListBox.SelectedItem as FriendHeader;
      if (selectedItem == null)
        return;
      this.HandleUserSelection(selectedItem);
      this.allFriendsListBox.SelectedItem = null;
    }

    private void onlineFriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selectedItem = this.onlineFriendsListBox.SelectedItem as FriendHeader;
      if (selectedItem == null)
        return;
      this.HandleUserSelection(selectedItem);
      this.onlineFriendsListBox.SelectedItem = null;
    }

    private void mutualFriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selectedItem = this.mutualFriendsListBox.SelectedItem as FriendHeader;
      if (selectedItem == null)
        return;
      this.HandleUserSelection(selectedItem);
      this.mutualFriendsListBox.SelectedItem = null;
    }

    private void HandleUserSelection(FriendHeader selected)
    {
      if (this._mode == FriendsPageMode.Default)
        Navigator.Current.NavigateToUserProfile(selected.UserId, selected.User.Name, "", false);
      if (this._mode != FriendsPageMode.PickAndBack)
        return;
      ParametersRepository.SetParameterForId("PickedUser", selected);
      ((Page) this).NavigationService.GoBackSafe();
    }

    private void friendListsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      FriendHeader selectedItem = this.friendListsListBox.SelectedItem as FriendHeader;
      if (selectedItem == null)
        return;
      FriendsList friendsList = selectedItem.FriendsList;
      if (friendsList == null)
        return;
      if (friendsList.lid == -1L)
        Navigator.Current.NavigateToBirthdaysPage();
      else
        Navigator.Current.NavigateToFriendsList(friendsList.lid, friendsList.name);
      this.friendListsListBox.SelectedItem = null;
    }

    private void pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      this.UpdateAppBar();
    }

    private void pivot_LoadedPivotItem_1(object sender, PivotItemEventArgs e)
    {
      if (e.Item == this.pivotItemOnline && !this._loadedOnline)
      {
        this._loadedOnline = true;
        this.FriendsVM.OnlineFriendsVM.LoadData(false, false,  null, false);
      }
      if (e.Item == this.pivotItemLists && !this._loadedLists)
      {
        this._loadedLists = true;
        this.FriendsVM.FriendListsVM.LoadData(false, false,  null, false);
      }
      if (e.Item != this.pivotItemMutualFriends || this._loadedCommon)
        return;
      this._loadedCommon = true;
      this.FriendsVM.CommonFriendsVM.LoadData(false, false,  null, false);
    }

    private void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemAll && Enumerable.Any<Group<FriendHeader>>(this.FriendsVM.AllFriendsVM.Collection))
        this.allFriendsListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemOnline && Enumerable.Any<FriendHeader>(this.FriendsVM.OnlineFriendsVM.Collection))
        this.onlineFriendsListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemMutualFriends && Enumerable.Any<FriendHeader>(this.FriendsVM.CommonFriendsVM.Collection))
      {
        this.mutualFriendsListBox.ScrollToTop();
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemLists || !Enumerable.Any<FriendHeader>(this.FriendsVM.FriendListsVM.Collection))
          return;
        this.friendListsListBox.ScrollToTop();
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FriendsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemAll = (PivotItem) base.FindName("pivotItemAll");
      this.allFriendsListBox = (ExtendedLongListSelector) base.FindName("allFriendsListBox");
      this.pivotItemOnline = (PivotItem) base.FindName("pivotItemOnline");
      this.onlineFriendsListBox = (ExtendedLongListSelector) base.FindName("onlineFriendsListBox");
      this.pivotItemLists = (PivotItem) base.FindName("pivotItemLists");
      this.friendListsListBox = (ExtendedLongListSelector) base.FindName("friendListsListBox");
      this.pivotItemMutualFriends = (PivotItem) base.FindName("pivotItemMutualFriends");
      this.mutualFriendsListBox = (ExtendedLongListSelector) base.FindName("mutualFriendsListBox");
    }
  }
}
