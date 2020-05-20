using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.DataObjects;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Library.Events;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKClient.Groups.Library;

namespace VKClient.Groups
{
  public class GroupsListPage : PageBase
  {
    private bool _isInitialized;
    private long _excludedId;
    private ApplicationBarIconButton _appBarButtonSearch;
    private ApplicationBarIconButton _appBarButtonGlobe;
    private ApplicationBarIconButton _appBarButtonCreate;
    private ApplicationBar _defaultAppBar;
    private DialogService _dialogService;
    private bool _pickManaged;
    private long _ownerId;
    private long _picId;
    private string _text;
    private IShareContentDataProvider _shareContentDataProvider;
    private bool _isGif;
    private string _accessKey;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal ExtendedLongListSelector communitiesListBox;
    internal PivotItem pivotItemEvents;
    internal ExtendedLongListSelector eventsListBox;
    internal PivotItem pivotItemManage;
    internal ExtendedLongListSelector manageListBox;
    private bool _contentLoaded;

    private GroupsListViewModel GroupsListVM
    {
      get
      {
        return base.DataContext as GroupsListViewModel;
      }
    }

    public GroupsListPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri=(uri1);
      string pageAppBarSearch = CommonResources.FriendsPage_AppBar_Search;
      applicationBarIconButton1.Text=(pageAppBarSearch);
      this._appBarButtonSearch = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/New/globe.png", UriKind.Relative);
      applicationBarIconButton2.IconUri=(uri2);
      string lowerInvariant1 = CommonResources.RecommendedGroups_Recommendations.ToLowerInvariant();
      applicationBarIconButton2.Text=(lowerInvariant1);
      this._appBarButtonGlobe = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("/Resources/appbar.add.rest.png", UriKind.Relative);
      applicationBarIconButton3.IconUri=(uri3);
      string lowerInvariant2 = CommonResources.NewFriendsList_Create.ToLowerInvariant();
      applicationBarIconButton3.Text=(lowerInvariant2);
      this._appBarButtonCreate = applicationBarIconButton3;
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor=(appBarBgColor);
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor=(appBarFgColor);
      this._defaultAppBar = applicationBar;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.communitiesListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.eventsListBox);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.manageListBox);
      this.communitiesListBox.OnRefresh = (Action) (() => this.GroupsListVM.AllVM.LoadData(true, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false));
      this.eventsListBox.OnRefresh = (Action) (() => this.GroupsListVM.EventsVM.LoadData(true, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false));
      this.manageListBox.OnRefresh = (Action) (() => this.GroupsListVM.ManagedVM.LoadData(true, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false));
      this.Header.OnHeaderTap = (Action) (() => this.OnHeaderTap());
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      this._appBarButtonSearch.Click+=(new EventHandler(this._appBarButtonSearch_Click));
      this._appBarButtonGlobe.Click+=(new EventHandler(this._appBarButtonGlobe_Click));
      this._appBarButtonCreate.Click+=(new EventHandler(GroupsListPage._appBarButtonCreate_Click));
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonSearch);
    }

    private void _appBarButtonGlobe_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToGroupRecommendations(0, "");
    }

    private void UpdateAppBar()
    {
      if (this.IsMenuOpen)
        return;
      this.ApplicationBar=((IApplicationBar) this._defaultAppBar);
      this._defaultAppBar.Opacity=(0.9);
      if (this._pickManaged || this._shareContentDataProvider != null || this._ownerId != 0L && this._ownerId != AppGlobalStateManager.Current.LoggedInUserId || this._defaultAppBar.Buttons.Contains((object) this._appBarButtonGlobe))
        return;
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonGlobe);
      this._defaultAppBar.Buttons.Add((object) this._appBarButtonCreate);
    }

    private void _appBarButtonSearch_Click(object sender, EventArgs e)
    {
        this._dialogService = new DialogService
        {
            BackgroundBrush = new SolidColorBrush(Colors.Transparent),
            HideOnNavigation = false,
            AnimationType = DialogService.AnimationTypes.None
        };
        ObservableCollection<GroupHeader> collection = this.GroupsListVM.AllVM.Collection;
        if (this._pickManaged)
        {
            collection = this.GroupsListVM.ManagedVM.Collection;
        }
        GroupsSearchDataProvider searchDataProvider = new GroupsSearchDataProvider(collection, this._excludedId, this._pickManaged);
        DataTemplate itemTemplate = (DataTemplate)Application.Current.Resources["VKGroupTemplate"];
        GenericSearchUC searchUC = new GenericSearchUC();
        searchUC.LayoutRootGrid.Margin=(new Thickness(0.0, 77.0, 0.0, 0.0));
        searchUC.Initialize<Group, GroupHeader>(searchDataProvider, new Action<object, object>(this.HandleSelectedItem), itemTemplate);
        searchUC.SearchTextBox.TextChanged+=(delegate(object s, TextChangedEventArgs ev)
        {
            bool flag = searchUC.SearchTextBox.Text != "";
            this.pivot.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
        });
        this._dialogService.Child = searchUC;
        this._dialogService.Show(this.pivot);
        CommunityOpenSource lastCurrentCommunitySource = CurrentCommunitySource.Source;
        this._dialogService.Closed += delegate(object p, EventArgs f)
        {
            CurrentCommunitySource.Source = lastCurrentCommunitySource;
        };
        CurrentCommunitySource.Source = CommunityOpenSource.Search;
    }


    private static void _appBarButtonCreate_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToCommunityCreation();
    }

    private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      this.HandleSelectedItem((object) longListSelector, longListSelector.SelectedItem);
      longListSelector.SelectedItem=((object) null);
    }

    private void HandleSelectedItem(object listBox, object selectedItem)
    {
      GroupHeader groupHeader = selectedItem as GroupHeader;
      if (groupHeader == null)
        return;
      if (this._pickManaged)
      {
        if (this._ownerId != 0L && this._picId != 0L)
          this.Share(this._text ?? "", groupHeader.Group.id, groupHeader.Group.name ?? "");
        ParametersRepository.SetParameterForId("PickedGroupForRepost", (object) groupHeader.Group);
        EventAggregator.Current.Publish((object) new PhotoIsRepostedInGroup());
        ((Page) this).NavigationService.GoBackSafe();
      }
      else if (this._shareContentDataProvider != null)
      {
        this._shareContentDataProvider.StoreDataToRepository();
        ShareContentDataProviderManager.StoreDataProvider(this._shareContentDataProvider);
        Navigator.Current.NavigateToNewWallPost(groupHeader.Group.id, true, groupHeader.Group.admin_level, groupHeader.Group.GroupType == GroupType.PublicPage, false, false);
      }
      else
        Navigator.Current.NavigateToGroup(groupHeader.Group.id, groupHeader.Group.name, false);
    }

    public void Share(string text, long gid = 0, string groupName = "")
    {
      if (!this._isGif)
      {
        WallService.Current.Repost(this._ownerId, this._picId, text, RepostObject.photo, gid, (Action<BackendResult<RepostResult, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Photo, gid, groupName);
          else
            new GenericInfoUC().ShowAndHideLater(CommonResources.Error, (FrameworkElement) null);
        }))));
      }
      else
      {
        string str = string.IsNullOrWhiteSpace(this._accessKey) ? string.Format("doc{0}_{1}", (object) this._ownerId, (object) this._picId) : string.Format("doc{0}_{1}_{2}", (object) this._ownerId, (object) this._picId, (object) this._accessKey);
        WallService current = WallService.Current;
        WallPostRequestData postData = new WallPostRequestData();
        postData.owner_id = -gid;
        postData.message = text;
        postData.AttachmentIds = new List<string>() { str };
        Action<BackendResult<ResponseWithId, ResultCode>> callback = (Action<BackendResult<ResponseWithId, ResultCode>>) (res => Execute.ExecuteOnUIThread((Action) (() =>
        {
          if (res.ResultCode == ResultCode.Succeeded)
            GenericInfoUC.ShowPublishResult(GenericInfoUC.PublishedObj.Doc, gid, groupName);
          else
            new GenericInfoUC().ShowAndHideLater(CommonResources.Error, (FrameworkElement) null);
        })));
        current.Post(postData, callback);
      }
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        string str = "";
        if (((Page) this).NavigationContext.QueryString.ContainsKey("Name"))
          str = ((Page) this).NavigationContext.QueryString["Name"];
        this._pickManaged = ((Page) this).NavigationContext.QueryString["PickManaged"] == bool.TrueString;
        this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
        this._excludedId = long.Parse(((Page) this).NavigationContext.QueryString["ExcludedId"]);
        this._ownerId = long.Parse(((Page) this).NavigationContext.QueryString["OwnerId"]);
        this._picId = long.Parse(((Page) this).NavigationContext.QueryString["PicId"]);
        this._isGif = bool.Parse(((Page) this).NavigationContext.QueryString["IsGif"]);
        this._accessKey = ((Page) this).NavigationContext.QueryString["AccessKey"];
        if (this._shareContentDataProvider is ShareExternalContentDataProvider)
          ((Page) this).NavigationService.ClearBackStack();
        long userId = this.CommonParameters.UserId;
        string userName = str;
        int num = this._pickManaged ? 1 : 0;
        long excludedId = this._excludedId;
        GroupsListViewModel groupsListViewModel = new GroupsListViewModel(userId, userName, num != 0, excludedId);
        base.DataContext=((object) groupsListViewModel);
        if (!this._pickManaged && this._shareContentDataProvider == null)
        {
          groupsListViewModel.AllVM.LoadData(false, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false);
        }
        else
        {
          this.Header.HideSandwitchButton = true;
          this.SuppressMenu = true;
          ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove((object) this.pivotItemAll);
          ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove((object) this.pivotItemEvents);
        }
        long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
        if (userId != loggedInUserId)
          ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove((object) this.pivotItemManage);
        this._isInitialized = true;
      }
      if (this._dialogService == null || !this._dialogService.IsOpen)
        this.UpdateAppBar();
      this._text = ParametersRepository.GetParameterForIdAndReset("ShareText") as string;
    }

    private void communitiesListBox_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.GroupsListVM.AllVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void eventsListBox_Link(object sender, LinkUnlinkEventArgs e)
    {
      this.GroupsListVM.EventsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void managedListBox_Link_1(object sender, LinkUnlinkEventArgs e)
    {
      this.GroupsListVM.ManagedVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void Canvas_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
    {
    }

    private void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemAll && this.GroupsListVM.AllVM.Collection.Any<GroupHeader>())
        this.communitiesListBox.ScrollToTop();
      else if (this.pivot.SelectedItem == this.pivotItemEvents && this.GroupsListVM.EventsVM.Collection.Any<Group<GroupHeader>>())
      {
        this.eventsListBox.ScrollToTop();
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemManage || !this.GroupsListVM.ManagedVM.Collection.Any<GroupHeader>())
          return;
        this.manageListBox.ScrollToTop();
      }
    }

    private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedItem == this.pivotItemEvents)
      {
        this.GroupsListVM.EventsVM.LoadData(false, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false);
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemManage)
          return;
        this.GroupsListVM.ManagedVM.LoadData(false, false, (Action<BackendResult<VKList<Group>, ResultCode>>) null, false);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Groups;component/GroupsListPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemAll = (PivotItem) base.FindName("pivotItemAll");
      this.communitiesListBox = (ExtendedLongListSelector) base.FindName("communitiesListBox");
      this.pivotItemEvents = (PivotItem) base.FindName("pivotItemEvents");
      this.eventsListBox = (ExtendedLongListSelector) base.FindName("eventsListBox");
      this.pivotItemManage = (PivotItem) base.FindName("pivotItemManage");
      this.manageListBox = (ExtendedLongListSelector) base.FindName("manageListBox");
    }
  }
}
