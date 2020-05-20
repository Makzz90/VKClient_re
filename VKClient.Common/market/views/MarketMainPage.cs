using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Market.ViewModels;
using VKClient.Common.UC;
using VKClient.Common.Utils;

namespace VKClient.Common.Market.Views
{
  public class MarketMainPage : PageBase
  {
    private bool _isInitialized;
    private MarketMainViewModel _viewModel;
    private readonly ApplicationBarIconButton _searchButton;
    private readonly ApplicationBarIconButton _searchParamsButton;
    internal GenericHeaderUC ucHeader;
    internal PullToRefreshUC ucPullToRefresh;
    internal ExtendedLongListSelector listBoxFeed;
    private bool _contentLoaded;

    public MarketMainPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("/Resources/appbar.feature.search.rest.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarSearch = CommonResources.AppBar_Search;
      applicationBarIconButton1.Text = appBarSearch;
      this._searchButton = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("/Resources/AppBarSearchParams.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarSearchFilter = CommonResources.AppBar_SearchFilter;
      applicationBarIconButton2.Text = appBarSearchFilter;
      this._searchParamsButton = applicationBarIconButton2;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = (CommonResources.Market.ToUpperInvariant());
      this.ucHeader.OnHeaderTap = new Action(this.Header_OnTap);
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxFeed);
      this.listBoxFeed.OnRefresh = (Action) (() => this._viewModel.ReloadData());
      this.BuildAppBar();
    }

    private void Header_OnTap()
    {
      this.listBoxFeed.ScrollToTop();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      this._searchButton.Click+=(new EventHandler(this.Search_OnTap));
      this._searchParamsButton.Click+=(new EventHandler(this.SetFilters_OnTap));
      applicationBar.Buttons.Add(this._searchButton);
      applicationBar.Buttons.Add(this._searchParamsButton);
      this.ApplicationBar = ((IApplicationBar) applicationBar);
    }

    private void Search_OnTap(object sender, EventArgs e)
    {
      this._viewModel.ShowSearch();
    }

    private void SetFilters_OnTap(object sender, EventArgs e)
    {
      this._viewModel.ShowSearch();
      this._viewModel.OpenSearchParams();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isInitialized)
      {
        this._viewModel = new MarketMainViewModel(long.Parse(((Page) this).NavigationContext.QueryString["OwnerId"]));
        this._viewModel.ReloadData();
        base.DataContext = this._viewModel;
        CurrentMarketItemSource.Source = MarketItemSource.market;
        this._isInitialized = true;
      }
      if (!ProductsSearchResultsUC.IsShowed || ProductsSearchResultsUC.IsAnyParamSet())
        return;
      ProductsSearchResultsUC.Hide();
    }

    private void ListBoxFeed_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.FeedVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/MarketMainPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
      this.listBoxFeed = (ExtendedLongListSelector) base.FindName("listBoxFeed");
    }
  }
}
