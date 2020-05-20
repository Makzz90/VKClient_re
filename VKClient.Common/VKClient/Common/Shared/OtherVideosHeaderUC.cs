using System;
using System.Diagnostics;
using System.Windows;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Shared
{
  public class OtherVideosHeaderUC : UserControlVirtualizable
  {
    private bool _contentLoaded;

    public OtherVideosHeaderUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Shared/OtherVideosHeaderUC.xaml", UriKind.Relative));
    }
  }
}
