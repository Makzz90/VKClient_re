using System;
using System.Collections.Generic;
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
        return base.DataContext as GraffitiPalleteViewModel;
      }
    }

    public Color CurrentColor
    {
      get
      {
          return ((ColorViewModel)Enumerable.First<ColorViewModel>(this.ViewModel.Colors, (Func<ColorViewModel, bool>)(color => color.IsSelected))).Color;
      }
    }

    public event EventHandler<Color> ColorSelected;

    public GraffitiPalleteUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public void SetColor(string colorHex)
    {
      GraffitiPalleteViewModel viewModel = this.ViewModel;
      if (viewModel == null)
        return;
      IEnumerator<ColorViewModel> enumerator = viewModel.Colors.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          ColorViewModel current = enumerator.Current;
          int num = string.Equals(current.ColorHex, colorHex, (StringComparison) 3) ? 1 : 0;
          current.IsSelected = num != 0;
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
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
      IEnumerator<ColorViewModel> enumerator = this.ViewModel.Colors.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          ColorViewModel current = enumerator.Current;
          current.IsSelected = (current.Color== colorVM.Color);
          if (current.IsSelected)
          {
            // ISSUE: reference to a compiler-generated field
            EventHandler<Color> colorSelected = this.ColorSelected;
            if (colorSelected != null)
            {
              Color color = current.Color;
              colorSelected(this, color);
            }
          }
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiPalleteUC.xaml", UriKind.Relative));
    }
  }
}
