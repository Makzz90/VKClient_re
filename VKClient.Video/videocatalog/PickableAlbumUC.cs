using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Video.Library;

namespace VKClient.Video.VideoCatalog
{
  public class PickableAlbumUC : UserControl
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    private AlbumHeader VM
    {
      get
      {
        return base.DataContext as AlbumHeader;
      }
    }

    public PickableAlbumUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.VM == null)
        return;
      this.VM.IsSelected = !this.VM.IsSelected;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Video;component/VideoCatalog/PickableAlbumUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
