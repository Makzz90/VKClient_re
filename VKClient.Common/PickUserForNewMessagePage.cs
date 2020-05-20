using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base;
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
  public class PickUserForNewMessagePage : PageBase
  {
    private bool _isInitialized;
    private DialogService _dialogService;
    private bool _groupViewOpened;
    private readonly ApplicationBarIconButton _appBarButtonSearch;
    private readonly ApplicationBarIconButton _appBarButtonEnableSelection;
    private readonly ApplicationBarIconButton _appBarButtonCheck;
    private readonly ApplicationBarIconButton _appBarButtonCancel;
    private ApplicationBar _mainAppBar;
    private ApplicationBar _selectionAppBar;
    private bool _createChat;
    private long _initialUserId;
    private bool _goBackOnResult;
    private int _currentCountInChat;
    private bool _isGlobalSearchForbidden;
    private bool _creatingChat;
    internal ProgressIndicator progressIndicator;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Pivot pivot;
    internal PivotItem pivotItemAll;
    internal Grid ContentPanel;
    internal ExtendedLongListSelector allFriendsListBox;
    internal PivotItem pivotItemLists;
    internal ExtendedLongListSelector friendListsListBox;
    private bool _contentLoaded;

    private PickUserViewModel PickUserVM
    {
      get
      {
        return base.DataContext as PickUserViewModel;
      }
    }

    private bool ForbidGlobalSearch
    {
      get
      {
        if (this._currentCountInChat <= 0 && !this._createChat)
          return this._isGlobalSearchForbidden;
        return true;
      }
    }

    public PickUserForNewMessagePage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string pageAppBarSearch = CommonResources.FriendsPage_AppBar_Search;
      applicationBarIconButton1.Text = pageAppBarSearch;
      this._appBarButtonSearch = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/appbar.manage.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string barSelectSeveral = CommonResources.AppBar_SelectSeveral;
      applicationBarIconButton2.Text = barSelectSeveral;
      this._appBarButtonEnableSelection = applicationBarIconButton2;
      ApplicationBarIconButton applicationBarIconButton3 = new ApplicationBarIconButton();
      Uri uri3 = new Uri("/Resources/check.png", UriKind.Relative);
      applicationBarIconButton3.IconUri = uri3;
      string chatEditAppBarSave = CommonResources.ChatEdit_AppBar_Save;
      applicationBarIconButton3.Text = chatEditAppBarSave;
      this._appBarButtonCheck = applicationBarIconButton3;
      ApplicationBarIconButton applicationBarIconButton4 = new ApplicationBarIconButton();
      Uri uri4 = new Uri("/Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton4.IconUri = uri4;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton4.Text = appBarCancel;
      this._appBarButtonCancel = applicationBarIconButton4;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.allFriendsListBox.JumpListOpening += ((EventHandler) ((s, e) => this._groupViewOpened = true));
      this.allFriendsListBox.JumpListClosed += ((EventHandler) ((s, e) => this._groupViewOpened = false));
      this.Header.HideSandwitchButton = true;
      this.SuppressMenu = true;
    }

    private void BuildAppBar()
    {
      this._mainAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._selectionAppBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._mainAppBar.Buttons.Add(this._appBarButtonSearch);
      this._mainAppBar.Buttons.Add(this._appBarButtonEnableSelection);
      this._selectionAppBar.Buttons.Add(this._appBarButtonCheck);
      this._selectionAppBar.Buttons.Add(this._appBarButtonCancel);
      this._appBarButtonSearch.Click+=(new EventHandler(this.AppBarButtonSearch_Click));
      this._appBarButtonEnableSelection.Click+=(new EventHandler(this.AppBarButtonEnableSelection_Click));
      this._appBarButtonCheck.Click+=(new EventHandler(this.AppBarButtonCheck_Click));
      this._appBarButtonCancel.Click+=(new EventHandler(this.AppBarButtonCancel_Click));
      this.ApplicationBar = ((IApplicationBar) this._mainAppBar);
    }

    private void AppBarButtonEnableSelection_Click(object sender, EventArgs e)
    {
      this.PickUserVM.IsInSelectionMode = true;
      this.UpdateAppBar();
    }

    private void AppBarButtonCancel_Click(object sender, EventArgs e)
    {
      if (this.PickUserVM.PickUserMode == PickUserMode.PickForMessage)
      {
        this.PickUserVM.IsInSelectionMode = false;
        this.UpdateAppBar();
      }
      else
        Navigator.Current.GoBack();
    }

    private void AppBarButtonCheck_Click(object sender, EventArgs e)
    {
      List<FriendHeader> allSelected = this.PickUserVM.GetAllSelected();
      if (allSelected.Count <= 0)
        return;
      this.RespondToSelection((List<User>)Enumerable.ToList<User>(Enumerable.Select<FriendHeader, User>(Enumerable.Where<FriendHeader>(allSelected, (Func<FriendHeader, bool>)(fh => fh.User != null)), (Func<FriendHeader, User>)(fh => fh.User))), (List<FriendsList>)Enumerable.ToList<FriendsList>(Enumerable.Select<FriendHeader, FriendsList>(Enumerable.Where<FriendHeader>(allSelected, (Func<FriendHeader, bool>)(fh => fh.FriendsList != null)), (Func<FriendHeader, FriendsList>)(fh => fh.FriendsList))));
    }

    private void UpdateAppBar()
    {
      if (this.PickUserVM.IsInSelectionMode)
        this.ApplicationBar = ((IApplicationBar) this._selectionAppBar);
      else
        this.ApplicationBar = ((IApplicationBar) this._mainAppBar);
      if (this.PickUserVM.PickUserMode == PickUserMode.PickForPartner)
        this.ApplicationBar = ( null);
      if (this.PickUserVM.PickUserMode == PickUserMode.PickWithSearch || this.PickUserVM.PickUserMode == PickUserMode.PickForPrivacy || this.PickUserVM.PickUserMode == PickUserMode.PickForStickerPackGift)
        this._mainAppBar.Buttons.Remove(this._appBarButtonEnableSelection);
      this._appBarButtonCheck.IsEnabled = (this.PickUserVM.SelectedCount > 0);
    }

    private void AppBarButtonSearch_Click(object sender, EventArgs e)
{
	this._dialogService = new DialogService
	{
		BackgroundBrush = new SolidColorBrush(Colors.Transparent),
		HideOnNavigation = true,
		AnimationType = DialogService.AnimationTypes.None
	};
	IEnumerable<User> arg_60_0 = this.PickUserVM.AllFriendsRaw;
	Func<User, FriendHeader> arg_60_1 = new Func<User, FriendHeader>((f)=>new FriendHeader(f, false));
	
	UsersSearchDataProvider searchDataProvider = new UsersSearchDataProvider(Enumerable.Select<User, FriendHeader>(arg_60_0, arg_60_1), !this.ForbidGlobalSearch);
	DataTemplate itemTemplate = (DataTemplate)Application.Current.Resources["FriendItemTemplate"];
	GenericSearchUC searchUC = new GenericSearchUC();
	searchUC.LayoutRootGrid.Margin=new Thickness(0.0, 77.0, 0.0, 0.0);
	searchUC.Initialize<User, FriendHeader>(searchDataProvider, new Action<object, object>(this.HandleSelectedItem), itemTemplate);
	searchUC.SearchTextBox.TextChanged+=(delegate(object s, TextChangedEventArgs ev)
	{
		bool flag = searchUC.SearchTextBox.Text != string.Empty;
        this.pivot.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
	});
	this._dialogService.Child = searchUC;
	this._dialogService.Show(this.pivot);
}

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
        bool flag;
        if (queryString.ContainsKey("GoBackOnResult"))
        {
          string str1 = queryString["GoBackOnResult"];
          flag = true;
          string str2 = flag.ToString();
          if (str1 == str2)
          {
            this._goBackOnResult = true;
            goto label_8;
          }
        }
        if (queryString.ContainsKey("CreateChat"))
        {
          string str1 = queryString["CreateChat"];
          flag = true;
          string str2 = flag.ToString();
          if (str1 == str2)
          {
            this._createChat = true;
            if (queryString.ContainsKey("InitialUserId"))
              this._initialUserId = long.Parse(queryString["InitialUserId"]);
          }
        }
label_8:
        PickUserMode mode = PickUserMode.PickForMessage;
        if (queryString.ContainsKey("PickMode"))
        {
          // ISSUE: type reference
          mode = (PickUserMode) Enum.Parse(typeof (PickUserMode), queryString["PickMode"]);
        }
        if (queryString.ContainsKey("CurrentCountInChat"))
          this._currentCountInChat = int.Parse(queryString["CurrentCountInChat"]);
        int sexFilter = 0;
        if (queryString.ContainsKey("SexFilter"))
          sexFilter = int.Parse(queryString["SexFilter"]);
        long result = 0;
        if (queryString.ContainsKey("ProductId"))
          long.TryParse(queryString["ProductId"], out result);
        if (queryString.ContainsKey("IsGlobalSearchForbidden"))
        {
          string str1 = queryString["IsGlobalSearchForbidden"];
          flag = true;
          string str2 = flag.ToString();
          if (str1 == str2)
            this._isGlobalSearchForbidden = true;
        }
        PickUserViewModel pickUserViewModel = result == 0L ? new PickUserViewModel(mode, sexFilter) : new PickUserViewModel(result);
        if (queryString.ContainsKey("CustomTitle"))
          pickUserViewModel.CustomTitle = queryString["CustomTitle"];
        if (mode == PickUserMode.PickForMessage || mode == PickUserMode.PickForPartner || (mode == PickUserMode.PickWithSearch || mode == PickUserMode.PickForStickerPackGift))
          ((PresentationFrameworkCollection<object>) ((ItemsControl) this.pivot).Items).Remove(this.pivotItemLists);
        base.DataContext = pickUserViewModel;
        pickUserViewModel.PropertyChanged += new PropertyChangedEventHandler(this.OnPropertyChanged);
        pickUserViewModel.Friends.LoadData(false, false,  null, false);
        pickUserViewModel.Lists.LoadData(false, false,  null, false);
        this._isInitialized = true;
      }
      this.UpdateAppBar();
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "SelectedCount"))
        return;
      this.UpdateAppBar();
    }

    protected override void OnBackKeyPress(CancelEventArgs e)
    {
      base.OnBackKeyPress(e);
      if (!this.PickUserVM.IsInSelectionMode || this._groupViewOpened || this.PickUserVM.PickUserMode != PickUserMode.PickForMessage)
        return;
      e.Cancel = true;
      this.PickUserVM.IsInSelectionMode = false;
      this.UpdateAppBar();
    }

    private void FriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      ExtendedLongListSelector longListSelector = sender as ExtendedLongListSelector;
      object selectedItem = longListSelector.SelectedItem;
      this.HandleSelectedItem(longListSelector, selectedItem);
      longListSelector.SelectedItem = null;
    }

    private void HandleSelectedItem(object listBox, object selectedItem)
    {
      FriendHeader friendHeader = selectedItem as FriendHeader;
      int maxAllowedCount;
      if (this._createChat)
      {
        maxAllowedCount = VKConstants.MaxChatCount;
        if (this._initialUserId != 0L)
          --maxAllowedCount;
      }
      else
        maxAllowedCount = VKConstants.MaxChatCount - this._currentCountInChat;
      if (friendHeader == null)
        return;
      if (!this.PickUserVM.IsInSelectionMode)
      {
        List<User> users = new List<User>();
        users.Add(friendHeader.User);
        // ISSUE: variable of the null type
        
        this.RespondToSelection(users, null);
      }
      else
      {
        friendHeader.IsSelected = !friendHeader.IsSelected;
        if (this.PickUserVM.PickUserMode != PickUserMode.PickForMessage || this.PickUserVM.SelectedCount <= maxAllowedCount)
          return;
        this.ShowMessageBoxCannotAdd(maxAllowedCount);
        friendHeader.IsSelected = false;
      }
    }

    private void ShowMessageBoxCannotAdd(int maxAllowedCount)
    {
      MessageBox.Show(UIStringFormatterHelper.FormatNumberOfSomething(maxAllowedCount, CommonResources.YouCanSelectNoMoreOneFrm, CommonResources.YouCanSelectNoMoreTwoFourFrm, CommonResources.YouCanSelectNoMoreFiveFrm, true,  null, false));
    }

    private void RespondToSelection(List<User> users, List<FriendsList> lists)
    {
      if (ListExtensions.IsNullOrEmpty((IList) users) && ListExtensions.IsNullOrEmpty((IList) lists))
        return;
      if (this._goBackOnResult)
      {
        ParametersRepository.SetParameterForId("SelectedUsers", users);
        if (ListExtensions.NotNullAndHasAtLeastOneNonNullElement((IList) lists))
          ParametersRepository.SetParameterForId("SelectedLists", lists);
        ((Page) this).NavigationService.GoBackSafe();
      }
      else if (this._createChat || users.Count > 1)
          this.CreateChatAndProceed((List<long>)Enumerable.ToList<long>(Enumerable.Select<User, long>(users, (Func<User, long>)(u => u.uid))));
      else
        Navigator.Current.NavigateToConversation(((User) Enumerable.First<User>(users)).uid, false, true, "", 0, false);
    }

    private void CreateChatAndProceed(List<long> uids)
    {
      long loggedInUserId = AppGlobalStateManager.Current.LoggedInUserId;
      if (this._creatingChat)
        return;
      this._creatingChat = true;
      this.ShowProgressIndicator(true, CommonResources.FriendsAndContactsSearchPage_CreatingChat);
      List<long> longList = new List<long>()
      {
        loggedInUserId
      };
      if (this._initialUserId != 0L)
        longList.Add(this._initialUserId);
      longList.AddRange((IEnumerable<long>) uids);
      MessagesService.Instance.CreateChat(longList, (Action<BackendResult<VKClient.Audio.Base.ResponseWithId, ResultCode>>) (res =>
      {
        this._creatingChat = false;
        this.ShowProgressIndicator(false, "");
        if (res.ResultCode != ResultCode.Succeeded)
          ExtendedMessageBox.ShowSafe(CommonResources.FriendsAndContactsSearchPage_FailedToCreateChat);
        else
          Navigator.Current.NavigateToConversation(res.ResultData.response, true, true, "", 0, false);
      }));
    }

    private void ShowProgressIndicator(bool show, string text = "")
    {
      if (base.Dispatcher.CheckAccess())
        this.DoShowProgressIndicator(show, text);
      else
        base.Dispatcher.BeginInvoke((Action) (() => this.DoShowProgressIndicator(show, text)));
    }

    private void DoShowProgressIndicator(bool show, string text)
    {
      this.progressIndicator.IsIndeterminate = show;
      this.progressIndicator.IsVisible = show;
      if (!show)
        return;
      this.progressIndicator.Text = text;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/PickUserForNewMessagePage.xaml", UriKind.Relative));
      this.progressIndicator = (ProgressIndicator) base.FindName("progressIndicator");
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemAll = (PivotItem) base.FindName("pivotItemAll");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.allFriendsListBox = (ExtendedLongListSelector) base.FindName("allFriendsListBox");
      this.pivotItemLists = (PivotItem) base.FindName("pivotItemLists");
      this.friendListsListBox = (ExtendedLongListSelector) base.FindName("friendListsListBox");
    }
  }
}
