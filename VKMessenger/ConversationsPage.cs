using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;
using VKMessenger.Library;
using VKMessenger.Views;

namespace VKMessenger
{
  public class ConversationsPage : PageBase
  {
    private bool _isInitialized;
    private ApplicationBar _appBarConversations;
    private static ConversationsUC _conversationsUCInstance;
    private static int TotalCount;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal PullToRefreshUC ucPullToRefresh;
    internal Grid ContentPanel;
    private bool _contentLoaded;

    public static ConversationsUC ConversationsUCInstance
    {
      get
      {
        if (ConversationsPage._conversationsUCInstance == null)
        {
          ConversationsPage._conversationsUCInstance = new ConversationsUC();
          ConversationsPage._conversationsUCInstance.PreventFromClearing = true;
        }
        return ConversationsPage._conversationsUCInstance;
      }
      set
      {
        ConversationsPage._conversationsUCInstance = value;
      }
    }

    public ConversationsPage()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      double num = 0.9;
      applicationBar.Opacity = num;
      this._appBarConversations = applicationBar;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      ++ConversationsPage.TotalCount;
      this.InitializeComponent();
      this.BuildAppBar();
      this.Header.TextBlockTitle.Text = CommonResources.Messages_Title;
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    ~ConversationsPage()
    {
      --ConversationsPage.TotalCount;
    }

    private void OnHeaderTap()
    {
      ConversationsPage.ConversationsUCInstance.conversationsListBox.ScrollToBottom(false);
    }

    private void BuildAppBar()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton(new Uri("/Resources/appbar.add.rest.png", UriKind.Relative));
      applicationBarIconButton1.Click+=(new EventHandler(this.appBarButtonAdd_Click));
      applicationBarIconButton1.Text = CommonResources.AppBar_Add;
      this._appBarConversations.Buttons.Add(applicationBarIconButton1);
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton(new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative));
      applicationBarIconButton2.Text = CommonResources.AppBar_Search;
      this._appBarConversations.Buttons.Add(applicationBarIconButton2);
      applicationBarIconButton2.Click+=(new EventHandler(this.appBarButtonSearch_Click));
      this.ApplicationBar = ((IApplicationBar) this._appBarConversations);
    }

    private void appBarButtonSearch_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToConversationsSearch();
    }

    private void appBarButtonRefresh_Click(object sender, EventArgs e)
    {
      ConversationsViewModel.Instance.RefreshConversations(false);
    }

    private void appBarButtonAdd_Click(object sender, EventArgs e)
    {
      Navigator.Current.NavigateToPickUser(false, 0, false, 0, PickUserMode.PickForMessage, "", 0, false);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        base.DataContext = (new ConversationsViewModelTemp());
        this.ContentPanel.Children.Add(ConversationsPage.ConversationsUCInstance);
        this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) ConversationsPage.ConversationsUCInstance.conversationsListBox);
        ConversationsPage.ConversationsUCInstance.conversationsListBox.OnRefresh = (Action) (() => ConversationsPage.ConversationsUCInstance.ConversationsVM.RefreshConversations(false));
        if (ShareContentDataProviderManager.RetrieveDataProvider() is ShareExternalContentDataProvider)
          base.NavigationService.ClearBackStack();
        this._isInitialized = true;
      }
      ConversationsPage.ConversationsUCInstance.PrepareForViewIfNeeded();
    }

    protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
    {
      base.OnRemovedFromJournal(e);
      ((PresentationFrameworkCollection<UIElement>) ((Panel) this.ContentPanel).Children).Remove((UIElement) ConversationsPage.ConversationsUCInstance);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKMessenger;component/ConversationsPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.ContentPanel = (Grid) base.FindName("ContentPanel");
    }
  }
}
