using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Market.Views
{
  public class FloatActionsUC : UserControl
  {
    private bool _contentLoaded;

    public event RoutedEventHandler ContactSellerButtonClicked;/*
    {
      add
      {
        RoutedEventHandler routedEventHandler = this.ContactSellerButtonClicked;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.ContactSellerButtonClicked, (RoutedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
      remove
      {
        RoutedEventHandler routedEventHandler = this.ContactSellerButtonClicked;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.ContactSellerButtonClicked, (RoutedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
    }*/

    public FloatActionsUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void ContactSellerButton_OnClick(object sender, RoutedEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      RoutedEventHandler sellerButtonClicked = this.ContactSellerButtonClicked;
      if (sellerButtonClicked == null)
        return;
      object obj = sender;
      RoutedEventArgs routedEventArgs = e;
      sellerButtonClicked.Invoke(obj, routedEventArgs);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/Market/Views/FloatActionsUC.xaml", UriKind.Relative));
    }
  }
}
