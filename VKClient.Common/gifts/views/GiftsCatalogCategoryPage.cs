using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Common.Framework;
using VKClient.Common.Gifts.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Gifts.Views
{
  public class GiftsCatalogCategoryPage : PageBase
  {
    private bool _isInitialized;
    private GiftsCatalogCategoryPageViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBoxGifts;
    private bool _contentLoaded;

    public GiftsCatalogCategoryPage()
    {
      this.InitializeComponent();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxGifts);
      this.listBoxGifts.OnRefresh = (Action) (() => this._viewModel.Reload(false));
      this.ucHeader.OnHeaderTap = (Action) (() => this.listBoxGifts.ScrollToTop());
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (this._isInitialized)
        return;
      string title = "";
      string categoryName = "";
      long result1 = 0;
      bool result2 = false;
      IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
      if (queryString.ContainsKey("CategoryName"))
        categoryName = HttpUtility.UrlDecode(queryString["CategoryName"]);
      if (queryString.ContainsKey("Title"))
        title = HttpUtility.UrlDecode(queryString["Title"]);
      if (queryString.ContainsKey("UserOrChatId"))
        long.TryParse(queryString["UserOrChatId"], out result1);
      if (queryString.ContainsKey("IsChat"))
        bool.TryParse(queryString["IsChat"], out result2);
      this._viewModel = new GiftsCatalogCategoryPageViewModel(categoryName, title, result1, result2);
      base.DataContext = this._viewModel;
      this._viewModel.Reload(true);
      this._isInitialized = true;
    }

    private void Item_OnTap(object sender, EventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      GiftsSectionItemHeader sectionItemHeader = (frameworkElement != null ? frameworkElement.DataContext : null) as GiftsSectionItemHeader;
      if (sectionItemHeader == null)
        return;
      EventAggregator.Current.Publish(new GiftsPurchaseStepsEvent(GiftPurchaseStepsSource.catalog_page, GiftPurchaseStepsAction.gift_page));
      sectionItemHeader.NavigateToGiftSend();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Gifts/Views/GiftsCatalogCategoryPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBoxGifts = (ExtendedLongListSelector) base.FindName("listBoxGifts");
    }
  }
}
