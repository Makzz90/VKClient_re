using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.Framework;

namespace VKClient.Common.Shared
{
  public class SelectorOption : ViewModelBase
  {
    private string _id;
    private string _text;
    private bool _isHidden;
    private bool _isSelected;

    public string Id
    {
      get
      {
        return this._id;
      }
      set
      {
        this._id = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Id));
      }
    }

    public string Text
    {
      get
      {
        return this._text;
      }
      set
      {
        this._text = value;
        this.NotifyPropertyChanged<string>((System.Linq.Expressions.Expression<Func<string>>) (() => this.Text));
      }
    }

    public bool IsHidden
    {
      get
      {
        return this._isHidden;
      }
      set
      {
        this._isHidden = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsHidden));
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
        if (this._isSelected == value)
          return;
        this._isSelected = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>) (() => this.IsSelected));
        this.NotifyPropertyChanged<SolidColorBrush>((System.Linq.Expressions.Expression<Func<SolidColorBrush>>) (() => this.ForegroundBrush));
      }
    }

    public SolidColorBrush ForegroundBrush
    {
      get
      {
        if (!this.IsSelected)
          return (SolidColorBrush) Application.Current.Resources["PhoneCaptionGrayBrush"];
        return (SolidColorBrush) Application.Current.Resources["PhoneAccentBlushBrush"];
      }
    }
  }
}
