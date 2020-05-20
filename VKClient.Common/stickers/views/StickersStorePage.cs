using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Library;
using VKClient.Common.Localization;
using VKClient.Common.Stickers.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Stickers.Views
{
  public class StickersStorePage : PageBase
  {
    private readonly TimeSpan _autoSlideInterval = TimeSpan.FromSeconds(4.0);
    private readonly double _floatingTabsShowThreshold;
    private bool _isInitialized;
    private StickersStoreViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal ViewportControl viewportControl;
    internal StackPanel stackPanelContent;
    internal SlideView slideView;
    internal MyVirtualizingPanel2 panelStickersList;
    internal Border borderFloatingTabs;
    private bool _contentLoaded;

    public StickersStorePage()
    {
      this.InitializeComponent();
      this.ucHeader.textBlockTitle.Text = (CommonResources.StickersStore.ToUpperInvariant());
      this.ucHeader.OnHeaderTap = (Action) (() => this.panelStickersList.ScrollToBottom(false));
      this.panelStickersList.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.viewportControl), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelStickersList);
      this.viewportControl.BindViewportBoundsTo((FrameworkElement) this.stackPanelContent);
      this._floatingTabsShowThreshold = ((FrameworkElement) this.slideView).Height;
      this.UpdateAutoSlideInterval();
      this.viewportControl.ViewportChanged += ((EventHandler<ViewportChangedEventArgs>) ((sender, args) => this.UpdateFloatingTabsVisibility()));
      ((UIElement) this.borderFloatingTabs).Visibility = Visibility.Collapsed;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton(new Uri("/Resources/appbar.feature.settings.rest.png", UriKind.Relative));
      string lowerInvariant = CommonResources.MyStickers.ToLowerInvariant();
      applicationBarIconButton1.Text = lowerInvariant;
      ApplicationBarIconButton applicationBarIconButton2 = applicationBarIconButton1;
      applicationBarIconButton2.Click+=((EventHandler) ((sender, args) => Navigator.Current.NavigateToStickersManage()));
      applicationBar.Buttons.Add(applicationBarIconButton2);
      this.ApplicationBar = ((IApplicationBar) applicationBar);
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
      this._viewModel = new StickersStoreViewModel(result1, result2);
      base.DataContext = this._viewModel;
      this._viewModel.Reload(true);
      this._isInitialized = true;
    }

    private void UpdateFloatingTabsVisibility()
    {
      Border borderFloatingTabs = this.borderFloatingTabs;
      Rect viewport = this.viewportControl.Viewport;
      // ISSUE: explicit reference operation
      Visibility visiblity = (((Rect) @viewport).Y > this._floatingTabsShowThreshold).ToVisiblity();
      ((UIElement) borderFloatingTabs).Visibility = visiblity;
    }

    protected override void FullscreenLoadersCollectionChanged()
    {
      this.UpdateAutoSlideInterval();
    }

    protected override void FlyoutsCollectionChanged()
    {
      this.UpdateAutoSlideInterval();
    }

    private void UpdateAutoSlideInterval()
    {
      if (this.FullscreenLoaders.Count == 0 && this.Flyouts.Count == 0)
        this.slideView.AutoSlideInterval = new TimeSpan?(this._autoSlideInterval);
      else
        this.slideView.AutoSlideInterval = new TimeSpan?();
    }

    private void StoreBanner_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      StoreBannerHeader dataContext = ((FrameworkElement) sender).DataContext as StoreBannerHeader;
      if (dataContext == null)
        return;
      CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.banner;
      StickersPackView.Show(new StockItemHeader(dataContext.StockItem, false, 0, false), "store");
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersStorePage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.viewportControl = (ViewportControl) base.FindName("viewportControl");
      this.stackPanelContent = (StackPanel) base.FindName("stackPanelContent");
      this.slideView = (SlideView) base.FindName("slideView");
      this.panelStickersList = (MyVirtualizingPanel2) base.FindName("panelStickersList");
      this.borderFloatingTabs = (Border) base.FindName("borderFloatingTabs");
    }
  }
}
