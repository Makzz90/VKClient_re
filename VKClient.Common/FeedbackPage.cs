using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Common.Backend;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common
{
  public class FeedbackPage : PageBase
  {
    private bool _isInitilized;
    private ApplicationBar _defaultAppBar;
    private ApplicationBarIconButton _appBarButtonRefresh;
    private bool _triggeredLoadingComments;
    internal Grid LayoutRoot;
    internal GenericHeaderUC Header;
    internal Pivot pivot;
    internal PivotItem pivotItemFeedback;
    internal ViewportControl scrollFeedback;
    internal StackPanel stackPanelFeedback;
    internal MyVirtualizingPanel2 panelFeedback;
    internal PivotItem pivotItemComments;
    internal ViewportControl scrollComments;
    internal StackPanel stackPanelComments;
    internal MyVirtualizingPanel2 panelComments;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    private FeedbackViewModel FeedbackVM
    {
      get
      {
        return base.DataContext as FeedbackViewModel;
      }
    }

    public FeedbackPage()
    {
      ApplicationBar applicationBar = new ApplicationBar();
      Color appBarBgColor = VKConstants.AppBarBGColor;
      applicationBar.BackgroundColor = appBarBgColor;
      Color appBarFgColor = VKConstants.AppBarFGColor;
      applicationBar.ForegroundColor = appBarFgColor;
      this._defaultAppBar = applicationBar;
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton();
      Uri uri = new Uri("Resources/appbar.refresh.rest.png", UriKind.Relative);
      applicationBarIconButton.IconUri = uri;
      string conversationAppBarRefresh = CommonResources.Conversation_AppBar_Refresh;
      applicationBarIconButton.Text = conversationAppBarRefresh;
      this._appBarButtonRefresh = applicationBarIconButton;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.BuildAppBar();
      this.panelComments.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollComments), false);
      this.scrollComments.BindViewportBoundsTo((FrameworkElement) this.stackPanelComments);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelComments);
      this.panelFeedback.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.scrollFeedback), false);
      this.scrollFeedback.BindViewportBoundsTo((FrameworkElement) this.stackPanelFeedback);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelFeedback);
      this.Header.OnHeaderTap = new Action(this.OnHeaderTap);
    }

    private void panelFeedback_ScrollPositionChanged(object sender, MyVirtualizingPanel2.ScrollPositionChangedEventAgrs e)
    {
      if (e.ScrollHeight == 0.0 || e.ScrollHeight - e.CurrentPosition >= VKConstants.LoadMoreNewsThreshold)
        return;
      this.FeedbackVM.LoadFeedback(false);
    }

    private void BuildAppBar()
    {
      this._appBarButtonRefresh.Click+=(new EventHandler(this._appBarButtonRefresh_Click));
      this._defaultAppBar.Buttons.Add(this._appBarButtonRefresh);
      this._defaultAppBar.Opacity = 0.9;
    }

    private void UpdateAppBar()
    {
    }

    private void _appBarButtonRefresh_Click(object sender, EventArgs e)
    {
      if (this.pivot.SelectedItem == this.pivotItemFeedback)
        this.FeedbackVM.LoadFeedback(true);
      else
        this.FeedbackVM.LoadComments(true);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitilized)
        return;
      FeedbackViewModel vm = new FeedbackViewModel(this.panelFeedback, this.panelComments);
      base.DataContext = vm;
      vm.LoadFeedback(false);
      this.UpdateAppBar();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelFeedback);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.panelComments);
      this.panelFeedback.OnRefresh = (Action) (() => vm.FeedbackVM.LoadData(true, false,  null, false));
      this.panelComments.OnRefresh = (Action) (() => vm.CommentsVM.LoadData(true, false,  null, false));
      CountersManager.Current.ResetFeedback();
      this._isInitilized = true;
    }

    private void pivot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
    {
      if (this.pivot.SelectedItem != this.pivotItemComments || this._triggeredLoadingComments)
        return;
      this.FeedbackVM.LoadComments(false);
      this._triggeredLoadingComments = true;
    }

    public void OnHeaderTap()
    {
      if (this.pivot.SelectedItem == this.pivotItemFeedback)
      {
        this.panelFeedback.ScrollToBottom(false);
      }
      else
      {
        if (this.pivot.SelectedItem != this.pivotItemComments)
          return;
        this.panelComments.ScrollToBottom(false);
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/FeedbackPage.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.Header = (GenericHeaderUC) base.FindName("Header");
      this.pivot = (Pivot) base.FindName("pivot");
      this.pivotItemFeedback = (PivotItem) base.FindName("pivotItemFeedback");
      this.scrollFeedback = (ViewportControl) base.FindName("scrollFeedback");
      this.stackPanelFeedback = (StackPanel) base.FindName("stackPanelFeedback");
      this.panelFeedback = (MyVirtualizingPanel2) base.FindName("panelFeedback");
      this.pivotItemComments = (PivotItem) base.FindName("pivotItemComments");
      this.scrollComments = (ViewportControl) base.FindName("scrollComments");
      this.stackPanelComments = (StackPanel) base.FindName("stackPanelComments");
      this.panelComments = (MyVirtualizingPanel2) base.FindName("panelComments");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
