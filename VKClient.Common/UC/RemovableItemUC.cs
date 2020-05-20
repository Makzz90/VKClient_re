using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VKClient.Common.Library;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public class RemovableItemUC : UserControl
  {
    internal TextBox textBoxText;
    private bool _contentLoaded;

    public IRemovableWithText VM
    {
      get
      {
        return base.DataContext as IRemovableWithText;
      }
    }

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

    public RemovableItemUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.VM == null)
        return;
      this.VM.Text = this.textBoxText.Text;
    }

    private void RemoveOptionTap(object sender, System.Windows.Input.GestureEventArgs e)
    {
      if (this.VM == null)
        return;
      this.VM.Remove();
    }

    private void textBox_KeyUp(object sender, KeyEventArgs e)
    {
      TextBox textbox = sender as TextBox;
      if (textbox == null || string.IsNullOrWhiteSpace(textbox.Text) || e.Key != Key.Enter)
        return;
      TextBox nextTextBox = FramePageUtils.FindNextTextBox((DependencyObject) FramePageUtils.CurrentPage, textbox);
      if (nextTextBox == null || ((FrameworkElement) nextTextBox).Tag == null || !(((FrameworkElement) nextTextBox).Tag.ToString() == "RemovableTextBox"))
        return;
      ((Control) nextTextBox).Focus();
    }

    private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.GotFocus == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.GotFocus.Invoke(sender, e);
    }

    private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.LostFocus == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.LostFocus.Invoke(sender, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/RemovableItemUC.xaml", UriKind.Relative));
      this.textBoxText = (TextBox) base.FindName("textBoxText");
    }
  }
}
