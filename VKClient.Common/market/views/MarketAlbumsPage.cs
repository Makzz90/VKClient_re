using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Market.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Market.Views
{
  public class MarketAlbumsPage : PageBase
  {
    private bool _isInitialized;
    private MarketAlbumsViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBoxAlbums;
    private bool _contentLoaded;

    public MarketAlbumsPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.Market_Collections.ToUpperInvariant());
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxAlbums);
      this.listBoxAlbums.OnRefresh = (Action) (() => this._viewModel.ReloadData());
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxAlbums.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._viewModel = new MarketAlbumsViewModel(long.Parse(((Page) this).NavigationContext.QueryString["OwnerId"]));
        this._viewModel.ReloadData();
        base.DataContext = this._viewModel;
        this._isInitialized = true;
      }
      CurrentMarketItemSource.Source = MarketItemSource.market;
    }

    private void ListBoxAlbums_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.AlbumsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/MarketAlbumsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBoxAlbums = (ExtendedLongListSelector) base.FindName("listBoxAlbums");
    }
  }
}
