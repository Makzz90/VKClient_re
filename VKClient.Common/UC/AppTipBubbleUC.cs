using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class AppTipBubbleUC : UserControl
  {
    internal Grid LayoutRoot;
    internal Image imageTip;
    internal TextBlock textBlockTip;
    internal TextBlock textBlockOK;
    private bool _contentLoaded;

    public AppTipBubbleUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/AppTipBubbleUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.imageTip = (Image) base.FindName("imageTip");
      this.textBlockTip = (TextBlock) base.FindName("textBlockTip");
      this.textBlockOK = (TextBlock) base.FindName("textBlockOK");
    }
  }
}
