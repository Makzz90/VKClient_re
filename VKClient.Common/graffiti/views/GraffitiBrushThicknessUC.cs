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
  public class GraffitiBrushThicknessUC : UserControl
  {
    private bool _contentLoaded;

    private BrushThicknessPickerViewModel ViewModel
    {
      get
      {
        return base.DataContext as BrushThicknessPickerViewModel;
      }
    }

    public int CurrentThickness
    {
      get
      {
          return ((BrushThicknessViewModel)Enumerable.First<BrushThicknessViewModel>(this.ViewModel.ThicknessItems, (Func<BrushThicknessViewModel, bool>)(item => item.IsSelected))).Thickness;
      }
    }

    public event EventHandler<int> ThicknessSelected;

    public GraffitiBrushThicknessUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public void SetFillColor(Color color)
    {
      if (this.ViewModel == null)
        return;
      IEnumerator<BrushThicknessViewModel> enumerator = this.ViewModel.ThicknessItems.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
          enumerator.Current.FillBrush = (Brush) new SolidColorBrush(color);
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    public void SetThickness(int thickness)
    {
      if (this.ViewModel == null)
        return;
      IEnumerator<BrushThicknessViewModel> enumerator = this.ViewModel.ThicknessItems.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          BrushThicknessViewModel current = enumerator.Current;
          int num = current.Thickness == thickness ? 1 : 0;
          current.IsSelected = num != 0;
        }
      }
      finally
      {
        if (enumerator != null)
          enumerator.Dispose();
      }
    }

    private void Thickness_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.SelectThickness(((FrameworkElement) sender).DataContext as BrushThicknessViewModel);
    }

    private void SelectThickness(BrushThicknessViewModel thicknessVM)
    {
      if (thicknessVM == null)
        return;
      IEnumerator<BrushThicknessViewModel> enumerator = this.ViewModel.ThicknessItems.GetEnumerator();
      try
      {
        while (enumerator.MoveNext())
        {
          BrushThicknessViewModel current = enumerator.Current;
          current.IsSelected = current.Thickness == thicknessVM.Thickness;
          if (current.IsSelected)
          {
            // ISSUE: reference to a compiler-generated field
            EventHandler<int> thicknessSelected = this.ThicknessSelected;
            if (thicknessSelected != null)
            {
              int thickness = current.Thickness;
              thicknessSelected(this, thickness);
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiBrushThicknessUC.xaml", UriKind.Relative));
    }
  }
}
