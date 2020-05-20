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
        return base.DataContext as OwnerFullHeaderWithSubscribeViewModel;
      }
    }

    public OwnerFullHeaderWithSubscribeUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void SubscribeUnsubscribeButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.SubscribeUnsubscribe();
    }

    private void Owner_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      this.VM.NavigateToOwner();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Shared/OwnerFullHeaderWithSubscribeUC.xaml", UriKind.Relative));
    }
  }
}
