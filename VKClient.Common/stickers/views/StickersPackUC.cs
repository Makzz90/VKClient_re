using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library;

namespace VKClient.Common.Stickers.Views
{
  public class StickersPackUC : UserControl
  {
    internal Style ListBoxItemNavDotsStyle;
    internal SlideView slideView;
    internal ListBox listBoxNavDots;
    private bool _contentLoaded;

    public StickersPackUC()
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
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Stickers/Views/StickersPackUC.xaml", UriKind.Relative));
      this.ListBoxItemNavDotsStyle = (Style) base.FindName("ListBoxItemNavDotsStyle");
      this.slideView = (SlideView) base.FindName("slideView");
      this.listBoxNavDots = (ListBox) base.FindName("listBoxNavDots");
    }
  }
}
