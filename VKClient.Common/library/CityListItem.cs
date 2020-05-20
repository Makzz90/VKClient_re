using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Backend.DataObjects;
using VKClient.Common.Framework;

namespace VKClient.Common.Library
{
  public class CityListItem : INotifyPropertyChanged
  {
    private readonly SolidColorBrush _defaultBrush = Application.Current.Resources["PhoneContrastTitleBrush"] as SolidColorBrush;
    private readonly SolidColorBrush _selectedBrush = Application.Current.Resources["PhoneSidebarSelectedIconBackgroundBrush"] as SolidColorBrush;
    private bool _isSelected;
    private readonly City _city;

    public long Id
    {
      get
      {
        if (this._city != null)
          return this._city.id;
        return -1;
      }
    }

    public string Title
    {
      get
      {
        if (this._city != null)
          return this._city.name;
        return "";
      }
    }

    public string Subtitle { get; private set; }

    public Visibility SubtitleVisibility
    {
      get
      {
        if (!string.IsNullOrEmpty(this.Subtitle))
          return Visibility.Visible;
        return Visibility.Collapsed;
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

    public City City
    {
      get
      {
        return this._city;
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
        this.NotifyPropertyChanged<bool>(() => this.IsSelected);
        this.NotifyPropertyChanged<SolidColorBrush>(() => this.Foreground);
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public CityListItem(City city)
    {
      this._city = city;
      this.Subtitle = this.GetSubtitle();
    }

    private string GetSubtitle()
    {
      if (this._city == null)
        return "";
      List<string> stringList = new List<string>();
      if (!string.IsNullOrEmpty(this._city.area))
        stringList.Add(this._city.area);
      if (!string.IsNullOrEmpty(this._city.region))
        stringList.Add(this._city.region);
      return string.Join(", ", (IEnumerable<string>) stringList);
    }

    public override string ToString()
    {
      return this.Title;
    }

    private void NotifyPropertyChanged<T>(Expression<Func<T>> propertyExpression)
    {
        if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
        {
            MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
            string propertyName = memberExpression.Member.Name;
            if (this.PropertyChanged != null)
            {
                Execute.ExecuteOnUIThread(delegate
                {
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }
                });
            }
        }
    }
  }
}
