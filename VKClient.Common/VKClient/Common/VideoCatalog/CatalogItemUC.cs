using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;
using VKClient.Video.Library;

namespace VKClient.Common.VideoCatalog
{
  public class CatalogItemUC : UserControlVirtualizable
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public Grid GridLayoutRoot
    {
      get
      {
        return this.LayoutRoot;
      }
    }

    private CatalogItemViewModel VM
    {
      get
      {
        return this.DataContext as CatalogItemViewModel;
      }
    }

    private VideoHeader VHVM
    {
      get
      {
        return this.DataContext as VideoHeader;
      }
    }

    private AlbumHeader AHVM
    {
      get
      {
        return this.DataContext as AlbumHeader;
      }
    }

    public CatalogItemUC()
    {
      this.InitializeComponent();
    }

    private void Grid_Tap(object sender, GestureEventArgs e)
    {
      if (this.VM != null)
        this.VM.HandleTap();
      else if (this.VHVM != null)
      {
        this.VHVM.HandleTap();
      }
      else
      {
        if (this.AHVM == null)
          return;
        this.AHVM.HandleTap();
      }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
      if (this.AHVM == null)
        return;
      this.AHVM.HandleEdit();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
      if (this.AHVM == null)
        return;
      this.AHVM.HandleDelete();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/VideoCatalog/CatalogItemUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) this.FindName("LayoutRoot");
    }
  }
}
