using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;
using VKClient.Video.Library;

namespace VKClient.Common.VideoCatalog
{
  public class CatalogHorizontalItemExtUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    private CatalogItemViewModel VM
    {
      get
      {
        return base.DataContext as CatalogItemViewModel;
      }
    }

    private AlbumHeader AlbumHeaderVM
    {
      get
      {
        return base.DataContext as AlbumHeader;
      }
    }

    public CatalogHorizontalItemExtUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.VM != null)
      {
        this.VM.HandleTap();
      }
      else
      {
        if (this.AlbumHeaderVM == null)
          return;
        this.AlbumHeaderVM.HandleTap();
      }
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/CatalogHorizontalItemExtUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
