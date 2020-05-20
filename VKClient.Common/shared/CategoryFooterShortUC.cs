using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.Shared
{
  public class CategoryFooterShortUC : UserControlVirtualizable
  {
    public const double FIXED_HEIGHT = 64.0;
    private bool _contentLoaded;

    public Action TapAction { get; set; }

    public CategoryFooterShortUC()
    {
      this.InitializeComponent();
    }

    private void Border_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.TapAction == null)
        return;
      this.TapAction();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Shared/CategoryFooterShortUC.xaml", UriKind.Relative));
    }
  }
}
