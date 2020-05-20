using System;
using System.Diagnostics;
using System.Windows;
using VKClient.Common.Framework;

namespace VKClient.Common.Payments.Views
{
  public class BalancePage : PageBase
  {
    private bool _contentLoaded;

    public BalancePage()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Payments/Views/BalancePage.xaml", UriKind.Relative));
    }
  }
}
