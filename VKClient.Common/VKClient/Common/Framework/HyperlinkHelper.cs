using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public static class HyperlinkHelper
  {
    public static Hyperlink GenerateHyperlink(string text, string tag, Action<Hyperlink, string> clickedCallback, Brush foregroundBrush = null)
    {
      Hyperlink h = new Hyperlink();
      HyperlinkHelper.SetState(h, HyperlinkState.Normal, foregroundBrush);
      h.Inlines.Add((Inline) new Run() { Text = text });
      h.Click += (RoutedEventHandler) ((s, e) => clickedCallback(h, tag));
      return h;
    }

    public static HyperlinkState GetState(Hyperlink h)
    {
      return (HyperlinkState) Enum.Parse(typeof (HyperlinkState), h.TargetName, false);
    }

    public static void SetState(Hyperlink h, HyperlinkState state, Brush foregroundBrush = null)
    {
      h.TargetName = state.ToString();
      if (state == HyperlinkState.Accent)
        HyperlinkHelper.SetAccentStyleForHyperlink(h);
      else
        HyperlinkHelper.SetAccentStyleForHyperlink(h);
    }

    private static void SetUnderlinedStyleForHyperlink(Hyperlink h, Brush foregroundBrush = null)
    {
      if (foregroundBrush == null)
        foregroundBrush = (Brush) (Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush);
      h.Foreground = foregroundBrush;
      SolidColorBrush solidColorBrush = new SolidColorBrush(((SolidColorBrush) foregroundBrush).Color);
      solidColorBrush.Opacity = 0.67;
      h.MouseOverForeground = (Brush) solidColorBrush;
      h.TextDecorations = TextDecorations.Underline;
      h.MouseOverTextDecorations = TextDecorations.Underline;
    }

    private static void SetAccentStyleForHyperlink(Hyperlink h)
    {
      SolidColorBrush solidColorBrush1 = Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush;
      SolidColorBrush solidColorBrush2 = new SolidColorBrush(solidColorBrush1.Color);
      solidColorBrush2.Opacity = 0.667;
      h.Foreground = (Brush) solidColorBrush1;
      h.MouseOverForeground = (Brush) solidColorBrush2;
      h.MouseOverTextDecorations = (TextDecorationCollection) null;
      h.TextDecorations = (TextDecorationCollection) null;
    }
  }
}
