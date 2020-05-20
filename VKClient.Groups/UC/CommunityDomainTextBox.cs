using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VKClient.Groups.UC
{
  public class CommunityDomainTextBox : UserControl
  {
      public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CommunityDomainTextBox), new PropertyMetadata("", new PropertyChangedCallback(CommunityDomainTextBox.TextPropertyChangedCallback)));
    internal Button FocusOwner;
    internal Border BackgroundBorder;
    internal TextBox ContentBox;
    private bool _contentLoaded;

    public string Text
    {
      get
      {
        return (string) base.GetValue(CommunityDomainTextBox.TextProperty);
      }
      set
      {
        base.SetValue(CommunityDomainTextBox.TextProperty, value);
      }
    }

    public event TextChangedEventHandler TextChanged;/*
    {
      add
      {
        TextChangedEventHandler changedEventHandler = this.TextChanged;
        TextChangedEventHandler comparand;
        do
        {
          comparand = changedEventHandler;
          changedEventHandler = Interlocked.CompareExchange<TextChangedEventHandler>(ref this.TextChanged, (TextChangedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (changedEventHandler != comparand);
      }
      remove
      {
        TextChangedEventHandler changedEventHandler = this.TextChanged;
        TextChangedEventHandler comparand;
        do
        {
          comparand = changedEventHandler;
          changedEventHandler = Interlocked.CompareExchange<TextChangedEventHandler>(ref this.TextChanged, (TextChangedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (changedEventHandler != comparand);
      }
    }*/

    public event RoutedEventHandler GotFocus;/*
    {
      add
      {
        RoutedEventHandler routedEventHandler = this.GotFocus;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.GotFocus, (RoutedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
      remove
      {
        RoutedEventHandler routedEventHandler = this.GotFocus;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.GotFocus, (RoutedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
    }*/

    public event RoutedEventHandler LostFocus;/*
    {
      add
      {
        RoutedEventHandler routedEventHandler = this.LostFocus;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.LostFocus, (RoutedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
      remove
      {
        RoutedEventHandler routedEventHandler = this.LostFocus;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.LostFocus, (RoutedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
    }*/

    public CommunityDomainTextBox()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void OnClicked(object sender, System.Windows.Input.GestureEventArgs e)
    {
      base.Focus();
    }

    private void ContentBox_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter)
        return;
      this.FocusOwner.Focus();
    }

    private void ContentBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      this.BackgroundBorder.BorderBrush=((Brush) Application.Current.Resources["PhoneTextBoxDefaultFocusedBorderBrush"]);
      // ISSUE: reference to a compiler-generated field
      RoutedEventHandler gotFocus = this.GotFocus;
      if (gotFocus == null)
        return;
      object obj = sender;
      RoutedEventArgs routedEventArgs = e;
      gotFocus.Invoke(obj, routedEventArgs);
    }

    private void ContentBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.BackgroundBorder.BorderBrush=((Brush) Application.Current.Resources["PhoneTextBoxDefaultBorderBrush"]);
      // ISSUE: reference to a compiler-generated field
      RoutedEventHandler lostFocus = this.LostFocus;
      if (lostFocus == null)
        return;
      object obj = sender;
      RoutedEventArgs routedEventArgs = e;
      lostFocus.Invoke(obj, routedEventArgs);
    }

    private void ContentBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this.Text = this.ContentBox.Text;
      // ISSUE: reference to a compiler-generated field
      TextChangedEventHandler textChanged = this.TextChanged;
      if (textChanged == null)
        return;
      TextChangedEventArgs changedEventArgs = e;
      textChanged.Invoke(this, changedEventArgs);
    }

    private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      TextBox contentBox = ((CommunityDomainTextBox) sender).ContentBox;
      // ISSUE: explicit reference operation
      string newValue = (string) e.NewValue;
      if (!(contentBox.Text != newValue))
        return;
      contentBox.Text = newValue;
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/UC/CommunityDomainTextBox.xaml", UriKind.Relative));
      this.FocusOwner = (Button) base.FindName("FocusOwner");
      this.BackgroundBorder = (Border) base.FindName("BackgroundBorder");
      this.ContentBox = (TextBox) base.FindName("ContentBox");
    }
  }
}
