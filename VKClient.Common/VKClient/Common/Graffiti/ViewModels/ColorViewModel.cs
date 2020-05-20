using System;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class ColorViewModel : ViewModelBase
  {
    private static readonly bool _isDarkTheme = new ThemeHelper().PhoneDarkThemeVisibility == Visibility.Visible;
    private bool _isSelected;

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>) (() => this.SelectedVisibility));
        this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>) (() => this.ExtraStroke));
      }
    }

    public bool HasExtraStroke { private get; set; }

    public Color Color { get; set; }

    public string ColorHex { get; set; }

    public Visibility SelectedVisibility
    {
      get
      {
        return this._isSelected.ToVisiblity();
      }
    }

    public int ExtraStroke
    {
      get
      {
        return ColorViewModel._isDarkTheme || !this.HasExtraStroke ? 0 : 1;
      }
    }

    public ColorViewModel(string color)
    {
      this.ColorHex = color;
      this.Color = color.ToColor();
    }
  }
}
