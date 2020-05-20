using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Market.Views
{
  public class FloatActionsUC : UserControl
  {
    private bool _contentLoaded;

    public event RoutedEventHandler ContactSellerButtonClicked;

    public FloatActionsUC()
    {
      this.InitializeComponent();
    }

    private void ContactSellerButton_OnClick(object sender, RoutedEventArgs e)
    {
      RoutedEventHandler routedEventHandler = this.ContactSellerButtonClicked;
      if (routedEventHandler == null)
        return;
      object sender1 = sender;
      RoutedEventArgs e1 = e;
      routedEventHandler(sender1, e1);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Market/Views/FloatActionsUC.xaml", UriKind.Relative));
    }
  }
}
