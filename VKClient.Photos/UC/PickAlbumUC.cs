using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Framework;
using VKClient.Photos.Library;

namespace VKClient.Photos.UC
{
  public class PickAlbumUC : UserControl
  {
    private static PhotoPickerAlbumsViewModel _vmInstance;
    internal Grid LayoutRoot;
    internal ExtendedLongListSelector listBoxAlbums;
    private bool _contentLoaded;

    public static PhotoPickerAlbumsViewModel VM
    {
      get
      {
        if (PickAlbumUC._vmInstance == null)
          PickAlbumUC._vmInstance = new PhotoPickerAlbumsViewModel();
        return PickAlbumUC._vmInstance;
      }
    }

    public Action<string> SelectedAlbumCallback { get; set; }

    public PickAlbumUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    public void Initialize()
    {
      base.DataContext = PickAlbumUC.VM;
    }

    public void Cleanup()
    {
      base.DataContext = null;
    }

    private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      FrameworkElement frameworkElement = sender as FrameworkElement;
      AlbumHeaderTwoInARow dataContext = frameworkElement.DataContext as AlbumHeaderTwoInARow;
      if (dataContext == null)
        return;
      AlbumHeader albumHeader = frameworkElement.Tag.ToString() == "1" ? dataContext.AlbumHeader1 : dataContext.AlbumHeader2;
      if (albumHeader == null)
        return;
      this.SelectedAlbumCallback(albumHeader.AlbumId);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Photos;component/UC/PickAlbumUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.listBoxAlbums = (ExtendedLongListSelector) base.FindName("listBoxAlbums");
    }
  }
}
