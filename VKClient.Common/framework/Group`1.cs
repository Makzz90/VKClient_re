using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using VKClient.Common.Library;

namespace VKClient.Common.Framework
{
  public class Group<T> : ObservableCollection<T>, IItemsCount
  {
    private readonly bool _isSingleGroup;
    private bool _isInSelectedState;
    private string _title;

    public bool IsInSelectedState
    {
      get
      {
        return this._isInSelectedState;
      }
      set
      {
        if (this._isInSelectedState == value)
          return;
        this._isInSelectedState = value;
        Execute.ExecuteOnUIThread((Action) (() =>
        {
          this.OnPropertyChanged(new PropertyChangedEventArgs("IsInSelectedState"));
          this.OnPropertyChanged(new PropertyChangedEventArgs("SelectionStateVisibility"));
        }));
      }
    }

    public int Id { get; set; }

    public Visibility SeparatorVisibility { get; set; }

    public Visibility SelectionStateVisibility
    {
      get
      {
        if (!this.IsInSelectedState)
          return Visibility.Collapsed;
        return Visibility.Visible;
      }
    }

    public string Title
    {
      get
      {
        return this._title;
      }
      set
      {
        this._title = value;
        this.OnPropertyChanged(new PropertyChangedEventArgs("Title"));
        this.OnPropertyChanged(new PropertyChangedEventArgs("HasTitleVisibility"));
      }
    }

    public Visibility HasTitleVisibility
    {
      get
      {
        if (!string.IsNullOrWhiteSpace(this.Title))
          return Visibility.Visible;
        return Visibility.Collapsed;
      }
    }

    public Thickness Margin
    {
      get
      {
        return new Thickness(0.0, this._isSingleGroup ? -16.0 : 8.0, 0.0, 16.0);
      }
    }

    public Group(string name, IEnumerable<T> items, bool isSingleGroup = false)
      : base(items)
    {
      this.Title = name;
      this._isSingleGroup = isSingleGroup;
    }

    public Group(string name, bool isSingleGroup = false)
    {
      this.Title = name;
      this._isSingleGroup = isSingleGroup;
    }

    public int GetItemsCount()
    {
      return this.Count;
    }
  }
}
