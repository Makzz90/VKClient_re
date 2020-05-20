using Microsoft.Phone.Shell;
using System;
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
      this.ucHeader.textBlockTitle.Text = CommonResources.StickersStore.ToUpperInvariant();
      this.ucHeader.OnHeaderTap = (Action) (() => this.panelStickersList.ScrollToBottom(false));
      this.panelStickersList.InitializeWithScrollViewer((IScrollableArea) new ViewportScrollableAreaAdapter(this.viewportControl), false);
      this.RegisterForCleanup((IMyVirtualizingPanel) this.panelStickersList);
      this.viewportControl.BindViewportBoundsTo((FrameworkElement) this.stackPanelContent);
      this._floatingTabsShowThreshold = this.slideView.Height;
      this.UpdateAutoSlideInterval();
      this.viewportControl.ViewportChanged += (EventHandler<ViewportChangedEventArgs>) ((sender, args) => this.UpdateFloatingTabsVisibility());
      this.borderFloatingTabs.Visibility = Visibility.Collapsed;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      ApplicationBarIconButton applicationBarIconButton = new ApplicationBarIconButton(new Uri("/Resources/appbar.feature.settings.rest.png", UriKind.Relative))
      {
        Text = CommonResources.MyStickers.ToLowerInvariant()
      };
      applicationBarIconButton.Click += (EventHandler) ((sender, args) => Navigator.Current.NavigateToStickersManage());
      applicationBar.Buttons.Add((object) applicationBarIconButton);
      this.ApplicationBar = (IApplicationBar) applicationBar;
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      this._viewModel = new StickersStoreViewModel();
      this.DataContext = (object) this._viewModel;
      this._viewModel.Reload();
      this._isInitialized = true;
    }

    private void UpdateFloatingTabsVisibility()
    {
      this.borderFloatingTabs.Visibility = (this.viewportControl.Viewport.Y > this._floatingTabsShowThreshold).ToVisiblity();
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

    private void StoreBanner_OnTap(object sender, GestureEventArgs e)
    {
      StoreBannerHeader storeBannerHeader = ((FrameworkElement) sender).DataContext as StoreBannerHeader;
      if (storeBannerHeader == null)
        return;
      CurrentStickersPurchaseFunnelSource.Source = StickersPurchaseFunnelSource.banner;
      StickersPackView.Show(storeBannerHeader.StockItem, "store");
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Stickers/Views/StickersStorePage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.viewportControl = (ViewportControl) this.FindName("viewportControl");
      this.stackPanelContent = (StackPanel) this.FindName("stackPanelContent");
      this.slideView = (SlideView) this.FindName("slideView");
      this.panelStickersList = (MyVirtualizingPanel2) this.FindName("panelStickersList");
      this.borderFloatingTabs = (Border) this.FindName("borderFloatingTabs");
    }
  }
}
