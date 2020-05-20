using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;

namespace VKClient.Common.UC
{
  public class SubscriptionItemHeaderUC : UserControl
  {
    private bool _contentLoaded;

    public SubscriptionItemHeaderUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void Item_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SubscriptionItemHeader dataContext = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (dataContext == null || dataContext.TapAction == null)
        return;
      dataContext.TapAction();
    }

    private void Subscribe_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SubscriptionItemHeader dataContext = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (dataContext == null || dataContext.SubscribeAction == null)
        return;
      dataContext.SubscribeAction();
    }

    private void Unsubscribe_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      SubscriptionItemHeader dataContext = ((FrameworkElement) sender).DataContext as SubscriptionItemHeader;
      if (dataContext == null || dataContext.UnsubscribeAction == null)
        return;
      dataContext.UnsubscribeAction();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/SubscriptionItemHeaderUC.xaml", UriKind.Relative));
    }
  }
}
