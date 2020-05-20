using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Profiles.Shared.Views
{
  public class ProfileAppPage : PageBase
  {
    private DialogService _ds = new DialogService();
    private bool _isInitialized;
    private ProfileAppPageViewModel _viewModel;
    private DateTime _pageOpenedDateTime;
    private SharePostUC _sharePostUC;
    internal Grid gridHeader;
    internal TextBlock textBlockTitle;
    internal Border borderMoreOptions;
    internal WebBrowser WebView;
    private bool _contentLoaded;

    public ProfileAppPage()
    {
      this.InitializeComponent();
      this.textBlockTitle.Text = ("");
      ((UIElement) this.borderMoreOptions).Visibility = Visibility.Collapsed;
      this.SuppressOpenMenuTapArea = true;
      this.BackKeyPress += ((EventHandler<CancelEventArgs>) ((sender, e) =>
      {
        if (!this.WebView.CanGoBack)
          return;
        e.Cancel = true;
        this.WebView.GoBack();
      }));
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      this._pageOpenedDateTime = DateTime.Now;
      if (!this._isInitialized)
      {
        this._isInitialized = true;
        base.DataContext = (new ViewModelBase());
        IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
        string utmParams = "";
        if (queryString.ContainsKey("UtmParams"))
          utmParams = queryString["UtmParams"];
        if (queryString.ContainsKey("AppId"))
        {
          long appId = long.Parse(queryString["AppId"]);
          long ownerId = 0;
          if (queryString.ContainsKey("OwnerId"))
            ownerId = long.Parse(queryString["OwnerId"]);
          ProfileAppPageViewModel appPageViewModel = new ProfileAppPageViewModel(appId, ownerId, utmParams);
          Action<ResultCode> action = new Action<ResultCode>(this.ReloadCallback);
          appPageViewModel.ReloadCallback = action;
          this._viewModel = appPageViewModel;
          base.DataContext = this._viewModel;
          this._viewModel.Reload(true);
        }
      }
      this.SetupSystemTray();
    }

    private void ReloadCallback(ResultCode resultCode)
    {
      if (this._viewModel == null)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (resultCode != ResultCode.Succeeded)
          return;
        Uri viewUri = this._viewModel.ViewUri;
        if (viewUri !=  null)
          this.WebView.Navigate(viewUri);
        if (!string.IsNullOrEmpty(this._viewModel.OriginalUrl))
          ((UIElement) this.borderMoreOptions).Visibility = Visibility.Visible;
        string screenTitle = this._viewModel.ScreenTitle;
        if (string.IsNullOrEmpty(screenTitle))
          return;
        this.textBlockTitle.Text = (screenTitle.ToUpperInvariant());
      }));
    }

    private void SetupSystemTray()
    {
      Execute.ExecuteOnUIThread((Action) (() => SystemTray.ForegroundColor = (((SolidColorBrush) Application.Current.Resources["PhoneGray500_Gray000Brush"]).Color)));
    }

    protected override void HandleOnNavigatingFrom(NavigatingCancelEventArgs e)
    {
      base.HandleOnNavigatingFrom(e);
      TimeSpan timeSpan = DateTime.Now - this._pageOpenedDateTime;
    }

    private void WebView_OnNavigating(object sender, NavigatingEventArgs e)
    {
      if (!e.Uri.AbsoluteUri.Contains("blank.html"))
        return;
      if (((Page) this).NavigationService.CanGoBack)
        ((Page) this).NavigationService.GoBackSafe();
      else
        Navigator.Current.NavigateToMainPage();
    }

    private void WebView_OnLoadCompleted(object sender, NavigationEventArgs e)
    {
      ((UIElement) this.WebView).Visibility = Visibility.Visible;
    }

    private void Close_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Navigator.Current.GoBack();
    }

    private void MoreOptions_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.ShowMoreOptions();
    }

    private void ShowMoreOptions()
    {
      List<MenuItem> menuItems = new List<MenuItem>();
      MenuItem menuItem1 = new MenuItem();
      string lowerInvariant1 = CommonResources.CopyLink.ToLowerInvariant();
      menuItem1.Header = lowerInvariant1;
      MenuItem menuItem2 = menuItem1;
      // ISSUE: method pointer
      menuItem2.Click += new RoutedEventHandler( this.MenuItemCopyLink_OnClick);
      menuItems.Add(menuItem2);
      MenuItem menuItem3 = new MenuItem();
      string lowerInvariant2 = CommonResources.ShareWallPost_Share.ToLowerInvariant();
      menuItem3.Header = lowerInvariant2;
      MenuItem menuItem4 = menuItem3;
      // ISSUE: method pointer
      menuItem4.Click += new RoutedEventHandler( this.MenuItemShare_OnClick);
      menuItems.Add(menuItem4);
      MenuItem menuItem5 = new MenuItem();
      string str = CommonResources.Report.ToLowerInvariant() + "...";
      menuItem5.Header = str;
      MenuItem menuItem6 = menuItem5;
      // ISSUE: method pointer
      menuItem6.Click += new RoutedEventHandler( this.MenuItemReport_OnClick);
      menuItems.Add(menuItem6);
      this.SetMenu(menuItems);
      this.ShowMenu();
    }

    private void SetMenu(List<MenuItem> menuItems)
    {
      if (menuItems == null || menuItems.Count == 0)
        return;
      ContextMenu contextMenu1 = new ContextMenu();
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) Application.Current.Resources["PhoneMenuBackgroundBrush"];
      ((Control) contextMenu1).Background = ((Brush) solidColorBrush1);
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) Application.Current.Resources["PhoneMenuForegroundBrush"];
      ((Control) contextMenu1).Foreground = ((Brush) solidColorBrush2);
      int num = 0;
      contextMenu1.IsZoomEnabled = num != 0;
      ContextMenu contextMenu2 = contextMenu1;
      foreach (MenuItem menuItem in menuItems)
        ((PresentationFrameworkCollection<object>) contextMenu2.Items).Add(menuItem);
      ContextMenuService.SetContextMenu((DependencyObject) this.gridHeader, contextMenu2);
    }

    private void ShowMenu()
    {
      ContextMenu contextMenu = ContextMenuService.GetContextMenu((DependencyObject) this.gridHeader);
      if (contextMenu == null)
        return;
      contextMenu.IsOpen = true;
    }

    private void MenuItemCopyLink_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      this._viewModel.CopyOriginalUrlToClipboard();
    }

    private void MenuItemReport_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      this._viewModel.ReportApp();
    }

    private void MenuItemShare_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (string.IsNullOrEmpty(this._viewModel.OriginalUrl))
        return;
      this.OpenSharePopup();
    }

    private void OpenSharePopup()
    {
      this._ds = new DialogService()
      {
        SetStatusBarBackground = false,
        HideOnNavigation = false
      };
      this._sharePostUC = new SharePostUC(0L);
      this._sharePostUC.SetShareEnabled(false);
      this._sharePostUC.SetShareCommunityEnabled(false);
      this._sharePostUC.SendTap += new EventHandler(this.ButtonSendWithMessage_Click);
      this._ds.Child = (FrameworkElement) this._sharePostUC;
      this._ds.AnimationType = DialogService.AnimationTypes.None;
      this._ds.AnimationTypeChild = DialogService.AnimationTypes.Swivel;
      SystemTray.ForegroundColor = (((SolidColorBrush) Application.Current.Resources["PhoneSystemTrayForegroundBrush"]).Color);
      this._ds.Closed += (EventHandler) ((sender, args) => this.SetupSystemTray());
      this._ds.Show( null);
    }

    private void ButtonSendWithMessage_Click(object sender, EventArgs eventArgs)
    {
      string text = this._sharePostUC.Text;
      this._ds.Hide();
      if (!string.IsNullOrEmpty(text))
        text += "\n";
      this.PrepareForSharing(text + this._viewModel.OriginalUrl);
      Navigator.Current.NavigateToPickConversation();
    }

    private void PrepareForSharing(string message)
    {
      ShareInternalContentDataProvider contentDataProvider = new ShareInternalContentDataProvider();
      contentDataProvider.Message = message;
      contentDataProvider.StoreDataToRepository();
      ShareContentDataProviderManager.StoreDataProvider((IShareContentDataProvider) contentDataProvider);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Profiles/Shared/Views/ProfileAppPage.xaml", UriKind.Relative));
      this.gridHeader = (Grid) base.FindName("gridHeader");
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.borderMoreOptions = (Border) base.FindName("borderMoreOptions");
      this.WebView = (WebBrowser) base.FindName("WebView");
    }
  }
}
