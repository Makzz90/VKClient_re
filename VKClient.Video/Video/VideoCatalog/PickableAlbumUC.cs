using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Video.Library;

namespace VKClient.Video.VideoCatalog
{
    public partial class PickableAlbumUC : UserControl
  {
    private AlbumHeader VM
    {
      get
      {
        return this.DataContext as AlbumHeader;
      }
    }

    public PickableAlbumUC()
    {
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, GestureEventArgs e)
    {
      if (this.VM == null)
        return;
      this.VM.IsSelected = !this.VM.IsSelected;
    }
  }
}
