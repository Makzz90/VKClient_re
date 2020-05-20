using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.Market.ViewModels
{
  public class ProductsSearchSortTypeListItem : ViewModelBase
  {
    private bool _isSelected;

    public string Text { get; set; }

    public ProductsSearchSortType SortType { get; set; }

    public bool IsRev { get; set; }

    public bool IsSeleted
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.TextForeground));
      }
    }

    public SolidColorBrush TextForeground
    {
      get
      {
        return (SolidColorBrush) Application.Current.Resources[this._isSelected ? (object) "PhoneAccentBlueBrush" : (object) "PhoneAlmostBlackBrush"];
      }
    }

    public ProductsSearchSortTypeListItem(string text, ProductsSearchSortType sortType = ProductsSearchSortType.Default, bool isRev = false)
    {
      this.Text = text;
      this.SortType = sortType;
      this.IsRev = isRev;
    }
  }
}
