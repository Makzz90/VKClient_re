using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Shared
{
  public class OwnerFullHeaderWithSubscribeUC : UserControl
  {
    public const double FixedHeight = 96.0;
    private bool _contentLoaded;

    private OwnerFullHeaderWithSubscribeViewModel VM
    {
      get
      {
        return this.DataContext as OwnerFullHeaderWithSubscribeViewModel;
      }
    }

    public OwnerFullHeaderWithSubscribeUC()
    {
      this.InitializeComponent();
    }

    private void SubscribeUnsubscribeButtonTap(object sender, GestureEventArgs e)
    {
      this.VM.SubscribeUnsubscribe();
    }

    private void Owner_OnTap(object sender, GestureEventArgs e)
    {
      this.VM.NavigateToOwner();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Shared/OwnerFullHeaderWithSubscribeUC.xaml", UriKind.Relative));
    }
  }
}
