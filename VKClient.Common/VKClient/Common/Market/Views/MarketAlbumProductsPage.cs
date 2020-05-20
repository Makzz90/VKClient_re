using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Market.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Market.Views
{
  public class MarketAlbumProductsPage : PageBase
  {
    private bool _isInitialized;
    private MarketAlbumProductsViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBoxProducts;
    private bool _contentLoaded;

    public MarketAlbumProductsPage()
    {
      this.InitializeComponent();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxProducts);
      this.listBoxProducts.OnRefresh = (Action) (() => this._viewModel.ReloadData());
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxProducts.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        IDictionary<string, string> queryString = this.NavigationContext.QueryString;
        string index1 = "OwnerId";
        long ownerId = long.Parse(queryString[index1]);
        string index2 = "AlbumId";
        long albumId = long.Parse(queryString[index2]);
        string index3 = "AlbumName";
        string str = queryString[index3];
        this._viewModel = new MarketAlbumProductsViewModel(ownerId, albumId);
        this._viewModel.Header = str.ToUpperInvariant();
        this._viewModel.ReloadData();
        this.DataContext = (object) this._viewModel;
        this._isInitialized = true;
      }
      CurrentMarketItemSource.Source = MarketItemSource.market;
    }

    private void ListBoxProducts_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.ProductsVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Market/Views/MarketAlbumProductsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) this.FindName("ucPullToRefresh");
      this.listBoxProducts = (ExtendedLongListSelector) this.FindName("listBoxProducts");
    }
  }
}
