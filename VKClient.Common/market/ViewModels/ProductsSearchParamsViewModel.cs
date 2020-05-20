using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using VKClient.Audio.Base.Events;
using VKClient.Audio.Base.Library;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Localization;
using VKClient.Common.Utils;
using VKClient.Audio.Base.Extensions;
namespace VKClient.Common.Market.ViewModels
{
  public class ProductsSearchParamsViewModel : ViewModelBase
  {
    private static readonly List<ProductsSearchSortTypeListItem> _sortTypes = new List<ProductsSearchSortTypeListItem>() { new ProductsSearchSortTypeListItem(CommonResources.Sort_ByDefault, ProductsSearchSortType.Default, false), new ProductsSearchSortTypeListItem(CommonResources.Sort_ByDate, ProductsSearchSortType.NewestFirst, true), new ProductsSearchSortTypeListItem(CommonResources.Sort_ByPriceLow, ProductsSearchSortType.ExpensiveFirst, false), new ProductsSearchSortTypeListItem(CommonResources.Sort_ByPriceHigh, ProductsSearchSortType.ExpensiveFirst, true) };
    private readonly long _priceMin;
    private readonly long _priceMax;
    private readonly string _currencyName;
    private readonly SearchParams _searchParams;
    private long _selectedPriceMin;
    private long _selectedPriceMax;

    public string TitleSort
    {
      get
      {
        return CommonResources.Order.ToUpperInvariant();
      }
    }

    public string TitlePrice
    {
      get
      {
        return CommonResources.Price.ToUpperInvariant();
      }
    }

    public List<ProductsSearchSortTypeListItem> SortTypes
    {
      get
      {
        return ProductsSearchParamsViewModel._sortTypes;
      }
    }

    public string PriceMinWatermark
    {
      get
      {
        if (this._priceMin != 0L)
          return UIStringFormatterHelper.FormatForUI(this._priceMin);
        return "0";
      }
    }

    public string PriceMaxWatermark
    {
      get
      {
        if (this._priceMax != 0L)
          return UIStringFormatterHelper.FormatForUI(this._priceMax);
        return "0";
      }
    }

    public string SelectedPriceMinStr
    {
      get
      {
        if (this._selectedPriceMin <= 0L)
          return "";
        return this._selectedPriceMin.ToString();
      }
      set
      {
        value = value.Replace(",", "");
        long result;
        this._selectedPriceMin = long.TryParse(value, out result) ? result : 0L;
        this.NotifyPropertyChanged("SelectedPriceMinStr");
        this.NotifyPropertyChanged<Visibility>(() => this.PriceMinWatermarkVisibility);
      }
    }

    public string SelectedPriceMaxStr
    {
      get
      {
        if (this._selectedPriceMax <= 0L)
          return "";
        return this._selectedPriceMax.ToString();
      }
      set
      {
        value = value.Replace(",", "");
        long result;
        this._selectedPriceMax = long.TryParse(value, out result) ? result : 0L;
        this.NotifyPropertyChanged("SelectedPriceMaxStr");
        this.NotifyPropertyChanged<Visibility>(() => this.PriceMaxWatermarkVisibility);
      }
    }

    public Visibility PriceMinWatermarkVisibility
    {
      get
      {
        return string.IsNullOrEmpty(this.SelectedPriceMinStr).ToVisiblity();
      }
    }

    public Visibility PriceMaxWatermarkVisibility
    {
      get
      {
        return string.IsNullOrEmpty(this.SelectedPriceMaxStr).ToVisiblity();
      }
    }

    public string CurrencyDesc
    {
      get
      {
        return this._currencyName.GetCurrencyDesc();
      }
    }

    public ProductsSearchParamsViewModel(long priceMin, long priceMax, string currencyName, SearchParams searchParams = null)
    {
      this._priceMin = priceMin / 100L;
      this._priceMax = priceMax / 100L;
      this._currencyName = currencyName;
      if (searchParams == null)
        searchParams = new SearchParams();
      this._searchParams = searchParams;
      this.UpdateSelectedItem();
      this.UpdatePrices();
    }

    private void UpdateSelectedItem()
    {
      ProductsSearchSortTypeListItem sortTypeItem = ProductsSearchParamsViewModel.GetSortTypeItem(this._searchParams.GetValue<ProductsSearchSortType>("sort"), this._searchParams.GetValue<bool>("rev"));
      if (sortTypeItem == null)
        return;
      this.SelectSortItem(sortTypeItem);
    }

    private void UpdatePrices()
    {
      long num1 = this._searchParams.GetValue<long>("price_from");
      long num2 = this._searchParams.GetValue<long>("price_to");
      if (num1 > 0L)
        this._selectedPriceMin = num1;
      if (num2 <= 0L)
        return;
      this._selectedPriceMax = num2;
    }

    private void SelectSortItem(int index)
    {
      if (index < 0 || index >= ProductsSearchParamsViewModel._sortTypes.Count)
        return;
      for (int index1 = 0; index1 < ProductsSearchParamsViewModel._sortTypes.Count; ++index1)
        ProductsSearchParamsViewModel._sortTypes[index1].IsSeleted = index1 == index;
    }

    public void SelectSortItem(ProductsSearchSortTypeListItem item)
    {
      this.SelectSortItem(ProductsSearchParamsViewModel._sortTypes.IndexOf(item));
    }

    public void Save()
    {
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method pointer
      ProductsSearchSortTypeListItem sortTypeListItem = (ProductsSearchSortTypeListItem) Enumerable.FirstOrDefault<ProductsSearchSortTypeListItem>(ProductsSearchParamsViewModel._sortTypes, new Func<ProductsSearchSortTypeListItem, bool>((t)=>{return t.IsSeleted;}));
      if (sortTypeListItem != null)
      {
        this._searchParams.SetValue<int>("sort", (int) sortTypeListItem.SortType, false);
        this._searchParams.SetValue<bool>("rev", sortTypeListItem.IsRev, false);
      }
      else
      {
        this._searchParams.ResetValue("sort");
        this._searchParams.ResetValue("rev");
      }
      if (this._selectedPriceMin > this._selectedPriceMax)
      {
        long selectedPriceMin = this._selectedPriceMin;
        this._selectedPriceMin = this._selectedPriceMax;
        this._selectedPriceMax = selectedPriceMin;
      }
      if (this._selectedPriceMin > 0L)
        this._searchParams.SetValue<long>("price_from", this._selectedPriceMin, false);
      else
        this._searchParams.ResetValue("price_from");
      if (this._selectedPriceMax > 0L)
        this._searchParams.SetValue<long>("price_to", this._selectedPriceMax, false);
      else
        this._searchParams.ResetValue("price_to");
      EventAggregator.Current.Publish(new SearchParamsUpdated(this._searchParams));
    }

    private static ProductsSearchSortTypeListItem GetSortTypeItem(ProductsSearchSortType sortType, bool isRev)
    {
        return Enumerable.FirstOrDefault<ProductsSearchSortTypeListItem>(ProductsSearchParamsViewModel._sortTypes, (ProductsSearchSortTypeListItem type) => type.SortType == sortType && type.IsRev == isRev);
    }

    public static string ToPrettyString(SearchParams searchParams, string currencyName)
    {
      List<string> stringList = new List<string>();
      ProductsSearchSortTypeListItem sortTypeItem = ProductsSearchParamsViewModel.GetSortTypeItem(searchParams.GetValue<ProductsSearchSortType>("sort"), searchParams.GetValue<bool>("rev"));
      if (sortTypeItem != null)
      {
        string lowerInvariant = sortTypeItem.Text.ToLowerInvariant();
        stringList.Add(lowerInvariant);
      }
      long num1 = searchParams.GetValue<long>("price_from");
      long num2 = searchParams.GetValue<long>("price_to");
      if (num1 > 0L || num2 > 0L)
      {
        string str = (num1 <= 0L || num2 <= 0L ? (num1 <= 0L ? string.Format("{0} {1}", CommonResources.UsersSearch_AgeTo, num2) : string.Format("{0} {1}", CommonResources.UsersSearch_AgeFrom, num1)) : string.Format("{0} - {1}", num1, num2)) + " " + currencyName.GetCurrencyDesc();
        stringList.Add(str);
      }
      return string.Join(", ", (IEnumerable<string>) stringList).Capitalize();
    }
  }
}
