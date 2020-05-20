using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.Market.ViewModels
{
  public class ProductsSearchSortTypeListItem : ViewModelBase
  {
    private bool _isSelected;

    public string Text { get; private set; }

    public ProductsSearchSortType SortType { get; private set; }

    public bool IsRev { get; private set; }

    public bool IsSeleted
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        base.NotifyPropertyChanged<SolidColorBrush>(() => this.TextForeground);
      }
    }

    public SolidColorBrush TextForeground
    {
      get
      {
        return (SolidColorBrush) Application.Current.Resources[this._isSelected ? "PhoneAccentBlueBrush" : "PhoneAlmostBlackBrush"];
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
