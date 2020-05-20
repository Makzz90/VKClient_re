using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VKClient.Common.Graffiti.ViewModels;

namespace VKClient.Common.Graffiti.Views
{
  public class GraffitiPalleteUC : UserControl
  {
    private bool _contentLoaded;

    private GraffitiPalleteViewModel ViewModel
    {
      get
      {
        return this.DataContext as GraffitiPalleteViewModel;
      }
    }

    public Color CurrentColor
    {
      get
      {
        return this.ViewModel.Colors.First<ColorViewModel>((Func<ColorViewModel, bool>) (color => color.IsSelected)).Color;
      }
    }

    public event EventHandler<Color> ColorSelected;

    public GraffitiPalleteUC()
    {
      this.InitializeComponent();
    }

    public void SetColor(string colorHex)
    {
      GraffitiPalleteViewModel viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      foreach (ColorViewModel color in viewModel.Colors)
      {
        int num = string.Equals(color.ColorHex, colorHex, StringComparison.InvariantCultureIgnoreCase) ? 1 : 0;
        color.IsSelected = num != 0;
      }
    }

    private void Color_OnMouseEnter(object sender, MouseEventArgs e)
    {
      this.SelectColor(((FrameworkElement) sender).DataContext as ColorViewModel);
    }

    private void SelectColor(ColorViewModel colorVM)
    {
      if (colorVM == null || colorVM.IsSelected)
        return;
      foreach (ColorViewModel color1 in this.ViewModel.Colors)
      {
        color1.IsSelected = color1.Color == colorVM.Color;
        if (color1.IsSelected)
        {
          EventHandler<Color> eventHandler = this.ColorSelected;
          if (eventHandler != null)
          {
            Color color2 = color1.Color;
            eventHandler((object) this, color2);
          }
        }
      }
    }

    private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
    {
      e.Handled = true;
    }

    private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
    {
      e.Handled = true;
    }

    private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiPalleteUC.xaml", UriKind.Relative));
    }
  }
}
