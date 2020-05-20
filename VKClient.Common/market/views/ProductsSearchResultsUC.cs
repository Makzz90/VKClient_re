using Microsoft.Phone.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.Framework;
using VKClient.Common.Framework.CodeForFun;
using VKClient.Common.Market.ViewModels;
using VKClient.Common.UC;

namespace VKClient.Common.Market.Views
{
  public class ProductsSearchResultsUC : UserControl, IHandle<SearchParamsUpdated>, IHandle
  {
    private ProductsSearchResultsViewModel _viewModel;
    private static DialogService _dialogService;
    private static ProductsSearchResultsUC _childUC;
    private static ProductsSearchResultsViewModel _childVM;
    internal TextBox textBoxSearch;
    internal TextBlock textBlockWatermarkText;
    internal Grid gridSearchResults;
    internal ExtendedLongListSelector listBoxProducts;
    internal PullToRefreshUC ucPullToRefresh;
    private bool _contentLoaded;

    public static bool IsShowed { get; private set; }

    public ProductsSearchResultsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucPullToRefresh.TrackListBox((ISupportPullToRefresh) this.listBoxProducts);
      this.listBoxProducts.OnRefresh = (Action) (() => this._viewModel.ReloadData());
      ((UIElement) this.gridSearchResults).Visibility = Visibility.Collapsed;
      EventAggregator.Current.Subscribe(this);
    }

    public static void Show(long ownerId, long priceFrom, long priceTo, int currencyId, string currencyName, SearchParams searchParams = null)
    {
      if (ProductsSearchResultsUC.IsShowed)
        return;
      ProductsSearchResultsUC.IsShowed = true;
      DialogService dialogService = new DialogService();
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.Black);
      double num1 = 0.4;
      ((Brush) solidColorBrush).Opacity = num1;
      dialogService.BackgroundBrush = (Brush) solidColorBrush;
      int num2 = 0;
      dialogService.HideOnNavigation = num2 != 0;
      int num3 = 1;
      dialogService.HasPopup = num3 != 0;
      int num4 = 6;
      dialogService.AnimationType = (DialogService.AnimationTypes) num4;
      ProductsSearchResultsUC._dialogService = dialogService;
      ProductsSearchResultsUC._childUC = new ProductsSearchResultsUC();
      ProductsSearchResultsUC._childVM = new ProductsSearchResultsViewModel(ownerId, priceFrom, priceTo, currencyId, currencyName, searchParams);
      ProductsSearchResultsUC._childUC._viewModel = ProductsSearchResultsUC._childVM;
      ((FrameworkElement) ProductsSearchResultsUC._childUC).DataContext = ProductsSearchResultsUC._childUC._viewModel;
      ProductsSearchResultsUC._dialogService.Opened += (EventHandler) ((sender, args) => ((Control) ProductsSearchResultsUC._childUC.textBoxSearch).Focus());
      ProductsSearchResultsUC._dialogService.Closed += (EventHandler) ((sender, args) => ProductsSearchResultsUC.IsShowed = false);
      ProductsSearchResultsUC._dialogService.Child = (FrameworkElement) ProductsSearchResultsUC._childUC;
      ProductsSearchResultsUC._dialogService.Show( null);
    }

    public static void Hide()
    {
      if (ProductsSearchResultsUC._dialogService == null)
        return;
      ProductsSearchResultsUC._dialogService.Hide();
    }

    public static bool IsAnyParamSet()
    {
      if (!(ProductsSearchResultsUC._childUC.textBoxSearch.Text != ""))
        return ProductsSearchResultsUC._childVM.SearchParamsViewModel.IsAnySet;
      return true;
    }

    private void TextBoxSearch_OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      ((Control) this).Focus();
    }

    private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      ((UIElement) this.textBlockWatermarkText).Visibility = (this.textBoxSearch.Text == "" ? Visibility.Visible : Visibility.Collapsed);
      if (this.textBoxSearch.Text == "" && !this._viewModel.SearchParamsViewModel.IsAnySet)
        ((UIElement) this.gridSearchResults).Visibility = Visibility.Collapsed;
      else
        ((UIElement) this.gridSearchResults).Visibility = Visibility.Visible;
      this._viewModel.Query = this.textBoxSearch.Text;
    }

    private void SearchParamsSummaryUC_OnClearButtonTap(object sender, EventArgs e)
    {
      if (!(this.textBoxSearch.Text == "") || this._viewModel.SearchParamsViewModel.IsAnySet)
        return;
      ((UIElement) this.gridSearchResults).Visibility = Visibility.Collapsed;
      ((Control) this.textBoxSearch).Focus();
    }

    private void TextBoxSearch_OnGotFocus(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.textBoxSearch.Text))
        return;
      this.textBoxSearch.SelectAll();
    }

    private void ListBoxProducts_OnLink(object sender, LinkUnlinkEventArgs e)
    {
      this._viewModel.SearchVM.LoadMoreIfNeeded(e.ContentPresenter.Content);
    }

    private void ListBoxProducts_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      ((Control) this.listBoxProducts).Focus();
    }

    public void Handle(SearchParamsUpdated message)
    {
      if (!message.SearchParams.IsAnySet)
        return;
      ((UIElement) this.gridSearchResults).Visibility = Visibility.Visible;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/ProductsSearchResultsUC.xaml", UriKind.Relative));
      this.textBoxSearch = (TextBox) base.FindName("textBoxSearch");
      this.textBlockWatermarkText = (TextBlock) base.FindName("textBlockWatermarkText");
      this.gridSearchResults = (Grid) base.FindName("gridSearchResults");
      this.listBoxProducts = (ExtendedLongListSelector) base.FindName("listBoxProducts");
      this.ucPullToRefresh = (PullToRefreshUC) base.FindName("ucPullToRefresh");
    }
  }
}
