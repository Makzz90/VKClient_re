using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class AdHeaderUC : UserControlVirtualizable
  {
    public const int HEIGHT = 66;
    internal TextBlock textBlock1;
    internal TextBlock textBlock2;
    private bool _contentLoaded;

    public AdHeaderUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AdHeaderUC.xaml", UriKind.Relative));
      this.textBlock1 = (TextBlock) base.FindName("textBlock1");
      this.textBlock2 = (TextBlock) base.FindName("textBlock2");
    }
  }
}
