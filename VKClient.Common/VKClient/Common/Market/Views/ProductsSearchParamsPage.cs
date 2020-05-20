using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
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
    private readonly ApplicationBarIconButton _appBarButtonSave = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/check.png", UriKind.Relative),
      Text = CommonResources.AppBarMenu_Save
    };
    private readonly ApplicationBarIconButton _appBarButtonReset = new ApplicationBarIconButton()
    {
      IconUri = new Uri("./Resources/appbar.cancel.rest.png", UriKind.Relative),
      Text = CommonResources.AppBar_Cancel
    };
    private bool _isIntialized;
    private ProductsSearchParamsViewModel _viewModel;
    internal GenericHeaderUC ucHeader;
    private bool _contentLoaded;

    public ProductsSearchParamsPage()
    {
      this.InitializeComponent();
      this.ucHeader.TextBlockTitle.Text = CommonResources.PageTitle_UsersSearch_SearchParameters;
      this.ucHeader.HideSandwitchButton = true;
      this.SuppressMenu = true;
      this.BuildAppBar();
    }

    private void BuildAppBar()
    {
      ApplicationBar applicationBar = ApplicationBarBuilder.Build(new Color?(), new Color?(), 0.9);
      applicationBar.Buttons.Add((object) this._appBarButtonSave);
      this._appBarButtonSave.Click += new EventHandler(this.AppBarButtonSave_OnClick);
      applicationBar.Buttons.Add((object) this._appBarButtonReset);
      this._appBarButtonReset.Click += new EventHandler(this.AppBarButtonReset_OnClick);
      this.ApplicationBar = (IApplicationBar) applicationBar;
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
        SearchParams searchParams = ParametersRepository.GetParameterForIdAndReset("ProductsSearchParams") as SearchParams;
        IDictionary<string, string> queryString = this.NavigationContext.QueryString;
        string index1 = "PriceFrom";
        long priceMin = long.Parse(queryString[index1]);
        string index2 = "PriceTo";
        long priceMax = long.Parse(queryString[index2]);
        string index3 = "CurrencyId";
        int.Parse(queryString[index3]);
        string index4 = "CurrencyName";
        string currencyName = queryString[index4];
        this._viewModel = new ProductsSearchParamsViewModel(priceMin, priceMax, currencyName, searchParams);
        this.DataContext = (object) this._viewModel;
        this._isIntialized = true;
      }
      CurrentMarketItemSource.Source = MarketItemSource.market;
    }

    private void SortItem_OnTap(object sender, GestureEventArgs e)
    {
      ProductsSearchSortTypeListItem sortTypeListItem = ((FrameworkElement) sender).DataContext as ProductsSearchSortTypeListItem;
      if (sortTypeListItem == null)
        return;
      this._viewModel.SelectSortItem(sortTypeListItem);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Market/Views/ProductsSearchParamsPage.xaml", UriKind.Relative));
      this.ucHeader = (GenericHeaderUC) this.FindName("ucHeader");
    }
  }
}
