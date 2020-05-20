using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.UC
{
  public class DateTimeCustomPickerUC : UserControl
  {
      public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(DateTimeCustomPickerUC), new PropertyMetadata(new PropertyChangedCallback(DateTimeCustomPickerUC.OnTitleChanged)));
      public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(DateTimeCustomPickerUC), new PropertyMetadata(new PropertyChangedCallback(DateTimeCustomPickerUC.OnContentTextChanged)));
    internal TextBlock textBlockTitle;
    internal Button buttonContent;
    private bool _contentLoaded;

    public string Title
    {
      get
      {
        return (string) base.GetValue(DateTimeCustomPickerUC.TitleProperty);
      }
      set
      {
        base.SetValue(DateTimeCustomPickerUC.TitleProperty, value);
      }
    }

    public string ContentText
    {
      get
      {
        return (string) base.GetValue(DateTimeCustomPickerUC.ContentTextProperty);
      }
      set
      {
        base.SetValue(DateTimeCustomPickerUC.ContentTextProperty, value);
      }
    }

    public event RoutedEventHandler Click;/*
    {
      add
      {
        RoutedEventHandler routedEventHandler = this.Click;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.Click, (RoutedEventHandler) Delegate.Combine((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
      remove
      {
        RoutedEventHandler routedEventHandler = this.Click;
        RoutedEventHandler comparand;
        do
        {
          comparand = routedEventHandler;
          routedEventHandler = Interlocked.CompareExchange<RoutedEventHandler>(ref this.Click, (RoutedEventHandler) Delegate.Remove((Delegate) comparand, (Delegate) value), comparand);
        }
        while (routedEventHandler != comparand);
      }
    }*/

    public DateTimeCustomPickerUC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DateTimeCustomPickerUC timeCustomPickerUc = d as DateTimeCustomPickerUC;
      if (timeCustomPickerUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      if (string.IsNullOrEmpty(newValue))
        return;
      timeCustomPickerUc.textBlockTitle.Text = newValue;
    }

    private static void OnContentTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      DateTimeCustomPickerUC timeCustomPickerUc = d as DateTimeCustomPickerUC;
      if (timeCustomPickerUc == null)
        return;
      // ISSUE: explicit reference operation
      string newValue = e.NewValue as string;
      if (string.IsNullOrEmpty(newValue))
        return;
      ((ContentControl) timeCustomPickerUc.buttonContent).Content = newValue;
    }

    private void ButtonContent_OnClicked(object sender, RoutedEventArgs e)
    {
      // ISSUE: reference to a compiler-generated field
      if (this.Click == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.Click.Invoke(this, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/DateTimeCustomPickerUC.xaml", UriKind.Relative));
      this.textBlockTitle = (TextBlock) base.FindName("textBlockTitle");
      this.buttonContent = (Button) base.FindName("buttonContent");
    }
  }
}
