using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Shared
{
  public class BinarySelectorUC : UserControl
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public BinarySelectorUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      ((sender as FrameworkElement).DataContext as SelectorOption).IsSelected = true;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Shared/BinarySelectorUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
