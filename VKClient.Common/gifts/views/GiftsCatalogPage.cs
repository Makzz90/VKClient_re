using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.Localization;
using VKClient.Common.UC;

namespace VKClient.Common.Gifts.Views
{
  public class GiftsCatalogPage : PageBase
  {
    private readonly UCPool _virtControlsPool = new UCPool();
    private bool _isInitialized;
    private GiftsCatalogViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ViewportControl viewportControl;
    internal StackPanel stackPanel;
    internal MyVirtualizingPanel2 virtPanel;
    private bool _contentLoaded;

    public GiftsCatalogPage()
    {
      this.InitializeComponent();
      this.ucHeader.Title = CommonResources.Gifts.ToUpperInvariant();
      this.ucHeader.OnHeaderTap = (Action) (() => this.virtPanel.ScrollToBottom(false));
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.virtPanel);
      this.viewportControl.BindViewportBoundsTo((FrameworkElement) this.stackPanel);
      this.virtPanel.OnRefresh = (Action) (() => this._viewModel.Reload(false));
      this.virtPanel.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.viewportControl), false);
      this.virtPanel.CreateVirtItemFunc = (Func<object, IVirtualizable>) (viewModel => (IVirtualizable) new UCItem(480.0,  new Thickness(), (Func<UserControlVirtualizable>) (() =>
      {
        GiftsCatalogCategoryUC fromPool = this._virtControlsPool.GetFromPool<GiftsCatalogCategoryUC>();
        object obj = viewModel;
        ((FrameworkElement) fromPool).DataContext = obj;
        return (UserControlVirtualizable) fromPool;
      }), (Func<double>) (() => 289.0), (Action<UserControlVirtualizable>) (control => this._virtControlsPool.AddBackToPool(control)), 0.0, false));
      this.RegisterForCleanup((IMyVirtualizingPanel) this.virtPanel);
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      long result1 = 0;
      bool result2 = false;
      if (queryString.ContainsKey("UserOrChatId"))
        long.TryParse(queryString["UserOrChatId"], out result1);
      if (queryString.ContainsKey("IsChat"))
        bool.TryParse(queryString["IsChat"], out result2);
      this._viewModel = new GiftsCatalogViewModel(result1, result2);
      base.DataContext = this._viewModel;
      this._viewModel.Reload(true);
      this._isInitialized = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftsCatalogPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.viewportControl = (ViewportControl) base.FindName("viewportControl");
      this.stackPanel = (StackPanel) base.FindName("stackPanel");
      this.virtPanel = (MyVirtualizingPanel2) base.FindName("virtPanel");
    }
  }
}
