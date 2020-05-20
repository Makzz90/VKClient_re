using System;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public static class MyTextMargin
  {
    private static double _ascent = 2210.0;
    private static double _descent = 514.0;
    private static double _lineSpacing = 2724.0;
    private static double _emHeight = 2048.0;
    public static readonly DependencyProperty TextMarginProperty = DependencyProperty.RegisterAttached("TextMargin", typeof(Thickness), typeof(MyTextMargin), new PropertyMetadata(new Thickness(double.NaN), new PropertyChangedCallback(MyTextMargin.OnTextMarginChanged)));

    public static Thickness GetTextMargin(TextBlock obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return (Thickness) ((DependencyObject) obj).GetValue(MyTextMargin.TextMarginProperty);
    }

    public static void SetTextMargin(TextBlock obj, Thickness value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      ((DependencyObject) obj).SetValue(MyTextMargin.TextMarginProperty, value);
    }

    private static void OnTextMarginChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      TextBlock textBlock = (TextBlock) o;
      // ISSUE: explicit reference operation
      Thickness newValue = (Thickness) e.NewValue;
      Thickness marginForTextMargin2 = MyTextMargin.GetRealMarginForTextMargin2(textBlock, newValue);
      ((FrameworkElement) textBlock).Margin = marginForTextMargin2;
    }

    public static Thickness GetRealMarginForTextMargin2(TextBlock textBlock, Thickness desiredTextMargin)
    {
      double fontSize = textBlock.FontSize;
      double emHeight = MyTextMargin._emHeight;
      double num1 = fontSize / emHeight;
      double ascent = MyTextMargin._ascent;
      double num2 = MyTextMargin._descent * num1;
      double num3 = MyTextMargin._lineSpacing * num1;
      double num4 = textBlock.FontFamily.Source == "Segoe WP Semilight" ? 3.0 : MyTextMargin._ascent / MyTextMargin._descent;
      if (textBlock.LineStackingStrategy == LineStackingStrategy.BlockLineHeight)
        num3 = textBlock.LineHeight;
      double num5 = num3 / (num4 + 1.0);
      double num6 = num3 - num5;
      double num7 = num2;
      double num8 = fontSize - num7;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return new Thickness(((Thickness) @desiredTextMargin).Left, ((Thickness) @desiredTextMargin).Top - (num6 - num8), ((Thickness) @desiredTextMargin).Right, ((Thickness) @desiredTextMargin).Bottom - num5);
    }
  }
}
