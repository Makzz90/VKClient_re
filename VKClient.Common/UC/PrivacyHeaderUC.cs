using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.UC
{
  public class PrivacyHeaderUC : UserControl
  {
    internal Grid LayoutRoot;
    private bool _contentLoaded;

    public Action OnTap { get; set; }

    public PrivacyHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.OnTap == null)
        return;
      this.OnTap();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/PrivacyHeaderUC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
    }
  }
}
