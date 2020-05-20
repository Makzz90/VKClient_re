using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;
using VKClient.Common.VideoCatalog;

namespace VKClient.Video.VideoCatalog
{
  public class CatalogItemCompactUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public CatalogItemCompactUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      (base.DataContext as CatalogItemViewModel).HandleTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/CatalogItemCompactUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
