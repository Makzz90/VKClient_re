using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using VKClient.Common.CommonExtensions;
using VKClient.Common.Framework;
using VKClient.Common.Utils;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class BrushThicknessViewModel : ViewModelBase
  {
    private static readonly bool _isDarkTheme = new ThemeHelper().PhoneDarkThemeVisibility == 0;
    private bool _isSelected;
    private Brush _fillBrush;

    public bool IsSelected
    {
      get
      {
        return this._isSelected;
      }
      set
      {
        this._isSelected = value;
        this.NotifyPropertyChanged<bool>((System.Linq.Expressions.Expression<Func<bool>>)(() => this.IsSelected));
        this.NotifyPropertyChanged<Visibility>((System.Linq.Expressions.Expression<Func<Visibility>>)(() => this.SelectedVisibility));
      }
    }

    public Brush FillBrush
    {
      get
      {
        if (DesignerProperties.IsInDesignTool)
          return (Brush) new SolidColorBrush("#ffe64646".ToColor());
        return this._fillBrush;
      }
      set
      {
        this._fillBrush = value;
        this.NotifyPropertyChanged("FillBrush");
        this.NotifyPropertyChanged<int>((System.Linq.Expressions.Expression<Func<int>>)(() => this.ExtraStroke));
      }
    }

    public int ExtraStroke
    {
      get
      {
        return BrushThicknessViewModel._isDarkTheme || !(this.FillBrush is SolidColorBrush) || !(((SolidColorBrush) this.FillBrush).Color== Colors.White) ? 0 : 1;
      }
    }

    public int Thickness { get; private set; }

    public int ViewThickness { get; private set; }

    public int StrokeThickness
    {
      get
      {
        return this.ViewThickness + 12;
      }
    }

    public Visibility SelectedVisibility
    {
      get
      {
        return this._isSelected.ToVisiblity();
      }
    }

    public BrushThicknessViewModel(int thickness, int viewThickness)
    {
      this.Thickness = thickness;
      this.ViewThickness = viewThickness;
    }
  }
}
