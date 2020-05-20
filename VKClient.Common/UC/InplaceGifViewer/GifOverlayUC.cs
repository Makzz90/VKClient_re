using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC.InplaceGifViewer
{
  public class GifOverlayUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    internal Grid gridText;
    private bool _contentLoaded;

    public Action OnTap { get; set; }

    public GifOverlayUC()
    {
      this.InitializeComponent();
    }

    private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnTap == null)
        return;
      this.OnTap();
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/InplaceGifViewer/GifOverlayUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.gridText = (Grid) base.FindName("gridText");
    }
  }
}
