using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.VideoCatalog
{
  public class VideoDurationUC : UserControl
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public VideoDurationUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/VideoCatalog/VideoDurationUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
