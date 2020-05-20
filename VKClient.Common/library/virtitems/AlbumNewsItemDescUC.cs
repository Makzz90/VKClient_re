using System;
using System.Diagnostics;
using System.Windows;

namespace VKClient.Common.Library.VirtItems
{
  public class AlbumNewsItemDescUC : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public AlbumNewsItemDescUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Library/VirtItems/AlbumNewsItemDescUC.xaml", UriKind.Relative));
    }
  }
}
