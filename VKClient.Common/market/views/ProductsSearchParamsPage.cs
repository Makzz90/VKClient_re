using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
  public class ProductsSearchParamsPage : PageBase
  {
    private bool _isIntialized;
    private ProductsSearchParamsViewModel _viewModel;
    private readonly ApplicationBarIconButton _appBarButtonSave;
    private readonly ApplicationBarIconButton _appBarButtonReset;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public ProductsSearchParamsPage()
    {
      ApplicationBarIconButton applicationBarIconButton1 = new ApplicationBarIconButton();
      Uri uri1 = new Uri("./Resources/check.png", UriKind.Relative);
      applicationBarIconButton1.IconUri = uri1;
      string appBarMenuSave = CommonResources.AppBarMenu_Save;
      applicationBarIconButton1.Text = appBarMenuSave;
      this._appBarButtonSave = applicationBarIconButton1;
      ApplicationBarIconButton applicationBarIconButton2 = new ApplicationBarIconButton();
      Uri uri2 = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative);
      applicationBarIconButton2.IconUri = uri2;
      string appBarCancel = CommonResources.AppBar_Cancel;
      applicationBarIconButton2.Text = appBarCancel;
      this._appBarButtonReset = applicationBarIconButton2;
      // ISSUE: explicit constructor call
      //base.\u002Ector();
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_SearchParameters;
      this.ucHeader.HideSandwitchButton = true;
      this.SuppressMenu = true;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      applicationBar.Buttons.Add(this._appBarButtonSave);
      this._appBarButtonSave.Click+=(new EventHandler(this.AppBarButtonSave_OnClick));
      applicationBar.Buttons.Add(this._appBarButtonReset);
      this._appBarButtonReset.Click+=(new EventHandler(this.AppBarButtonReset_OnClick));
      this.ApplicationBar = ((IApplicationBar) applicationBar);
    }

    private void AppBarButtonSave_OnClick(object sender, EventArgs eventArgs)
    {
      this._viewModel.Save();
      Navigator.Current.GoBack();
    }

    private void AppBarButtonReset_OnClick(object sender, EventArgs eventArgs)
    {
      Navigator.Current.GoBack();
    }

    protected override void HandleOnNavigatedTo(NavigationEventArgs e)
    {
      base.HandleOnNavigatedTo(e);
      if (!this._isIntialized)
      {
        SearchParams parameterForIdAndReset = ParametersRepository.GetParameterForIdAndReset("ProductsSearchParams") as SearchParams;
        IDictionary<string, string> queryString = ((Page) this).NavigationContext.QueryString;
        string index1 = "PriceFrom";
        long priceMin = long.Parse(queryString[index1]);
        string index2 = "PriceTo";
        long priceMax = long.Parse(queryString[index2]);
        string index3 = "CurrencyId";
        int.Parse(queryString[index3]);
        string index4 = "CurrencyName";
        string currencyName = queryString[index4];
        this._viewModel = new ProductsSearchParamsViewModel(priceMin, priceMax, currencyName, parameterForIdAndReset);
        base.DataContext = this._viewModel;
        this._isIntialized = true;
      }
      CurrentMarketItemSource.Source = MarketItemSource.market;
    }

    private void SortItem_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ProductsSearchSortTypeListItem dataContext = ((FrameworkElement) sender).DataContext as ProductsSearchSortTypeListItem;
      if (dataContext == null)
        return;
      this._viewModel.SelectSortItem(dataContext);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/ProductsSearchParamsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) base.FindName("ucHeader");
    }
  }
}
