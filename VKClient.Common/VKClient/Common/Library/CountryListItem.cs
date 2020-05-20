using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class CountryListItem : INotifyPropertyChanged
  {
    private readonly SolidColorBrush _defaultBrush = Application.Current.Resources["PhoneContrastTitleBrush"] as SolidColorBrush;
    private readonly SolidColorBrush _selectedBrush = Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"] as SolidColorBrush;
    private bool _isSelected;
    private readonly Country _country;

    public long Id
    {
      get
      {
        if (this._country != null)
          return this._country.id;
        return -1;
      }
    }

    public string Title
    {
      get
      {
        if (this._country != null)
          return this._country.name;
        return "";
      }
    }

    public SolidColorBrush Foreground
    {
      get
      {
        if (!this._isSelected)
          return this._defaultBrush;
        return this._selectedBrush;
      }
    }

    public Country Country
    {
      get
      {
        return this._country;
      }
    }

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSelected));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.Foreground));
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public CountryListItem(Country country)
    {
      this._country = country;
    }

    public override string ToString()
    {
      return this.Title;
    }

    private void NotifyPropertyChanged<T>(System.Linq.Expressions.Expression<Func<T>> propertyExpression)
    {
      if (propertyExpression.Body.NodeType != ExpressionType.MemberAccess)
        return;
      string propertyName = (propertyExpression.Body as MemberExpression).Member.Name;
      if (this.PropertyChanged == null)
        return;
      Execute.ExecuteOnUIThread((Action) (() =>
      {
        if (this.PropertyChanged == null)
          return;
        this.PropertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
      }));
    }
  }
}
