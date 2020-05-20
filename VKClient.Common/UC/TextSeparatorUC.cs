using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Library.VirtItems;

namespace VKClient.Common.UC
{
  public class TextSeparatorUC : UserControlVirtualizable
  {
      public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextSeparatorUC), new PropertyMetadata(new PropertyChangedCallback(TextSeparatorUC.Text_OnChanged)));
    public const double FIXED_HEIGHT = 56.0;
    internal Grid gridViewedFeedback;
    internal TextBlock textBlockText;
    private bool _contentLoaded;

    public string Text
    {
      get
      {
        return (string) base.GetValue(TextSeparatorUC.TextProperty);
      }
      set
      {
        base.SetValue(TextSeparatorUC.TextProperty, value);
      }
    }

    public TextSeparatorUC()
    {
      this.InitializeComponent();
    }

    private static void Text_OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      // ISSUE: explicit reference operation
      ((TextSeparatorUC) d).textBlockText.Text = (e.NewValue as string);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/TextSeparatorUC.xaml", UriKind.Relative));
      this.gridViewedFeedback = (Grid) base.FindName("gridViewedFeedback");
      this.textBlockText = (TextBlock) base.FindName("textBlockText");
    }
  }
}
