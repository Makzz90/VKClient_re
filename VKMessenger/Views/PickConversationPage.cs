using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKMessenger.Views
{
  public class PickConversationPage : PageBase
  {
    private bool _isInitialized;
    private readonly DialogService _de = new DialogService();
    private readonly ApplicationBarIconButton _appBarButtonSearch;
    private IShareContentDataProvider _shareContentDataProvider;
    internal Grid LayoutRoot;
    internal Grid ContentPanel;
    internal ConversationsUC conversationsUC;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    public PickConversationPage()
    {
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      Uri uri = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton.IconUri = uri;
      string appBarSearch = CommonResources.AppBar_Search;
      applicationBarIconButton.Text = appBarSearch;
      this._appBarButtonSearch = applicationBarIconButton;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.RegisterForCleanup((IMyVirtualizingPanel) this.conversationsUC.conversationsListBox);
      this.conversationsUC.IsLookup = true;
      this.BuildAppBar();
      this.Header.TextBlockTitle.Text = CommonResources.ChooseConversation;
      this.Header.HideSandwitchButton = true;
      this.Header.OnHeaderTap = (Action) (() => this.conversationsUC.conversationsListBox.ScrollToBottom(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.conversationsUC.conversationsListBox);
      this.conversationsUC.conversationsListBox.OnRefresh = (Action) (() => this.conversationsUC.ConversationsVM.RefreshConversations(false));
      this.SuppressMenu = true;
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar1 = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar1.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar1.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar1.Opacity = num;
      ApplicationBar applicationBar2 = applicationBar1;
      this._appBarButtonSearch.Click+=(new EventHandler(this._appBarButtonSearch_Click));
      applicationBar2.Buttons.Add(this._appBarButtonSearch);
      this.ApplicationBar = ((IApplicationBar) applicationBar2);
    }

    private void _appBarButtonSearch_Click(object sender, EventArgs e)
{
	GenericSearchUC searchUC = new GenericSearchUC();
	searchUC.LayoutRootGrid.Margin=(new Thickness(0.0, 32.0, 0.0, 0.0));
	GenericSearchUC arg_94_0 = searchUC;
	ISearchDataProvider<User, FriendHeader> arg_94_1 = new ConversationsSearchDataProvider();
	Action<object, object> arg_94_2 = new Action<object, object>((object listBox, object selectedItem)=>
	{
		FriendHeader friendHeader = selectedItem as FriendHeader;
		if (friendHeader != null)
		{
			bool isChat = friendHeader.UserId < 0L;
			long userOrChatId = Math.Abs(friendHeader.UserId);
			Navigator.Current.NavigateToConversation(userOrChatId, isChat, true, "", 0, false);
		}
	});
	
	arg_94_0.Initialize<User, FriendHeader>(arg_94_1, arg_94_2, Application.Current.Resources["FriendItemTemplate"] as DataTemplate);
	searchUC.SearchTextBox.TextChanged+=(delegate(object s, TextChangedEventArgs ev)
	{
		bool flag = searchUC.SearchTextBox.Text != "";
        this.ContentPanel.Visibility = (flag ? Visibility.Collapsed : Visibility.Visible);
	});
	this._de.HideOnNavigation = false;
	this._de.AnimationType = DialogService.AnimationTypes.None;
	this._de.BackgroundBrush = new SolidColorBrush(Colors.Transparent);
	this._de.Child = searchUC;
	this._de.Show(this.ContentPanel);
}

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      base.DataContext = (new ConversationsViewModelTemp());
      this.conversationsUC.PrepareForViewIfNeeded();
      this._shareContentDataProvider = ShareContentDataProviderManager.RetrieveDataProvider();
      this._isInitialized = true;
    }

    protected override void HandleOnNavigatedFrom(NavigationEventArgs e)
    {
      base.HandleOnNavigatedFrom(e);
      if (e.NavigationMode == NavigationMode.Back || this._shareContentDataProvider == null)
        return;
      this._shareContentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider(this._shareContentDataProvider);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/Views/PickConversationPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
      this.conversationsUC = (ConversationsUC) base.FindName("conversationsUC");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
