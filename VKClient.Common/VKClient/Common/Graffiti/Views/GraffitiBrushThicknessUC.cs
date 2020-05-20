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
  public class GraffitiBrushThicknessUC : UserControl
  {
    private bool _contentLoaded;

    private BrushThicknessPickerViewModel ViewModel
    {
      get
      {
        return this.DataContext as BrushThicknessPickerViewModel;
      }
    }

    public int CurrentThickness
    {
      get
      {
        return this.ViewModel.ThicknessItems.First<BrushThicknessViewModel>((Func<BrushThicknessViewModel, bool>) (item => item.IsSelected)).Thickness;
      }
    }

    public event EventHandler<int> ThicknessSelected;

    public GraffitiBrushThicknessUC()
    {
      this.InitializeComponent();
    }

    public void SetFillColor(Color color)
    {
      if (this.ViewModel == null)
        return;
      foreach (BrushThicknessViewModel thicknessItem in this.ViewModel.ThicknessItems)
        thicknessItem.FillBrush = (Brush) new SolidColorBrush(color);
    }

    public void SetThickness(int thickness)
    {
      if (this.ViewModel == null)
        return;
      foreach (BrushThicknessViewModel thicknessItem in this.ViewModel.ThicknessItems)
      {
        int num = thicknessItem.Thickness == thickness ? 1 : 0;
        thicknessItem.IsSelected = num != 0;
      }
    }

    private void Thickness_OnTap(object sender, GestureEventArgs e)
    {
      this.SelectThickness(((FrameworkElement) sender).DataContext as BrushThicknessViewModel);
    }

    private void SelectThickness(BrushThicknessViewModel thicknessVM)
    {
      if (thicknessVM == null)
        return;
      foreach (BrushThicknessViewModel thicknessItem in this.ViewModel.ThicknessItems)
      {
        thicknessItem.IsSelected = thicknessItem.Thickness == thicknessVM.Thickness;
        if (thicknessItem.IsSelected)
        {
          EventHandler<int> eventHandler = this.ThicknessSelected;
          if (eventHandler != null)
          {
            int thickness = thicknessItem.Thickness;
            eventHandler((object) this, thickness);
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
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Graffiti/Views/GraffitiBrushThicknessUC.xaml", UriKind.Relative));
    }
  }
}
