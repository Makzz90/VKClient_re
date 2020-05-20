using System;
using System.Windows;
using System.Windows.Controls;
using VKClient.Audio.Base.Extensions;

namespace VKClient.Common.Framework
{
  public class CorrectableTextBlock
  {
      public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(CorrectableTextBlock), new PropertyMetadata(new PropertyChangedCallback(CorrectableTextBlock.OnTextChanged)));

    public static string GetText(TextBlock block)
    {
      if (block == null)
        throw new ArgumentNullException();
      return (string) ((DependencyObject) block).GetValue(CorrectableTextBlock.TextProperty);
    }

    public static void SetText(TextBlock block, string value)
    {
      if (block == null)
        throw new ArgumentNullException();
      ((DependencyObject) block).SetValue(CorrectableTextBlock.TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      TextBlock textName = (TextBlock) o;
      // ISSUE: explicit reference operation
      string newValue = (string) e.NewValue;
      textName.Text = newValue;
      double maxWidth = ((FrameworkElement) textName).MaxWidth;
      textName.CorrectText(maxWidth);
    }
  }
}
