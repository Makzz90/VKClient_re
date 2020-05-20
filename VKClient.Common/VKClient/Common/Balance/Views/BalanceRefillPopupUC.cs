using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Balance.Views
{
  public class BalanceRefillPopupUC : UserControl
  {
    private bool _contentLoaded;

    public BalanceRefillPopupUC()
    {
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Balance/Views/BalanceRefillPopupUC.xaml", UriKind.Relative));
    }
  }
}
