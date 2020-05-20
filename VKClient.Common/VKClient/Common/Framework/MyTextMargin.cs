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
    public static readonly DependencyProperty TextMarginProperty = DependencyProperty.RegisterAttached("TextMargin", typeof (Thickness), typeof (MyTextMargin), new PropertyMetadata((object) new Thickness(double.NaN), new PropertyChangedCallback(MyTextMargin.OnTextMarginChanged)));

    public static Thickness GetTextMargin(TextBlock obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      return (Thickness) obj.GetValue(MyTextMargin.TextMarginProperty);
    }

    public static void SetTextMargin(TextBlock obj, Thickness value)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");
      obj.SetValue(MyTextMargin.TextMarginProperty, (object) value);
    }

    private static void OnTextMarginChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      TextBlock textBlock = (TextBlock) o;
      Thickness desiredTextMargin = (Thickness) e.NewValue;
      Thickness marginForTextMargin2 = MyTextMargin.GetRealMarginForTextMargin2(textBlock, desiredTextMargin);
      textBlock.Margin = marginForTextMargin2;
    }

    public static Thickness GetRealMarginForTextMargin2(TextBlock textBlock, Thickness desiredTextMargin)
    {
      double fontSize = textBlock.FontSize;
      double num1 = MyTextMargin._emHeight;
      double num2 = fontSize / num1;
      double num3 = MyTextMargin._ascent;
      double num4 = MyTextMargin._descent * num2;
      double num5 = MyTextMargin._lineSpacing * num2;
      double num6 = textBlock.FontFamily.Source == "Segoe WP Semilight" ? 3.0 : MyTextMargin._ascent / MyTextMargin._descent;
      if (textBlock.LineStackingStrategy == LineStackingStrategy.BlockLineHeight)
        num5 = textBlock.LineHeight;
      double num7 = num5 / (num6 + 1.0);
      double num8 = num5 - num7;
      double num9 = num4;
      double num10 = fontSize - num9;
      return new Thickness(desiredTextMargin.Left, desiredTextMargin.Top - (num8 - num10), desiredTextMargin.Right, desiredTextMargin.Bottom - num7);
    }
  }
}
