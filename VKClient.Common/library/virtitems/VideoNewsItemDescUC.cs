using System;
using System.Diagnostics;
using System.Windows;

namespace VKClient.Common.Library.VirtItems
{
  public class VideoNewsItemDescUC : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public VideoNewsItemDescUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Library/VirtItems/VideoNewsItemDescUC.xaml", UriKind.Relative));
    }
  }
}
