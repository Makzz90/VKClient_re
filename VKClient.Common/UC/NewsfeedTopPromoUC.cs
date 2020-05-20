using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class NewsfeedTopPromoUC : NewsfeedPromoUC
  {
    internal Grid gridBackground;
    internal Grid gridCutArea;
    internal Polygon polygonTriangle;
    internal Grid gridMessage;
    private bool _contentLoaded;

    private NewsfeedTopPromoViewModel ViewModel
    {
      get
      {
        return base.DataContext as NewsfeedTopPromoViewModel;
      }
    }

    public Action ButtonPrimaryTapCallback { get; set; }

    public Action ButtonSecondaryTapCallback { get; set; }

    public Action BackgroundTapCallback { get; set; }

    protected override FrameworkElement GridCutArea
    {
      get
      {
        return (FrameworkElement) this.gridCutArea;
      }
    }

    protected override Grid GridBackground
    {
      get
      {
        return this.gridBackground;
      }
    }

    protected override FrameworkElement GridMessage
    {
      get
      {
        return (FrameworkElement) this.gridMessage;
      }
    }

    protected override Polygon PolygonTriangle
    {
      get
      {
        return this.polygonTriangle;
      }
    }

    public NewsfeedTopPromoUC()
    {
      this.InitializeComponent();
    }

    private void ButtonPrimary_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      Action primaryTapCallback = this.ButtonPrimaryTapCallback;
      if (primaryTapCallback == null)
        return;
      primaryTapCallback();
    }

    private void ButtonSecondary_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      e.Handled = true;
      Action secondaryTapCallback = this.ButtonSecondaryTapCallback;
      if (secondaryTapCallback == null)
        return;
      secondaryTapCallback();
    }

    private void GridBackground_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      Action backgroundTapCallback = this.BackgroundTapCallback;
      if (backgroundTapCallback == null)
        return;
      backgroundTapCallback();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/NewsfeedTopPromoUC.xaml", UriKind.Relative));
      this.gridBackground = (Grid) base.FindName("gridBackground");
      this.gridCutArea = (Grid) base.FindName("gridCutArea");
      this.polygonTriangle = (Polygon) base.FindName("polygonTriangle");
      this.gridMessage = (Grid) base.FindName("gridMessage");
    }
  }
}
