using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace VKClient.Common.Framework
{
  public static class HyperlinkHelper
  {
      public static Hyperlink GenerateHyperlink(string text, string tag, Action<Hyperlink, string> clickedCallback, Brush foregroundBrush = null, HyperlinkState hyperlinkState = HyperlinkState.Normal)
      {
          Hyperlink h = new Hyperlink();
          HyperlinkHelper.SetState(h, hyperlinkState, foregroundBrush);
          PresentationFrameworkCollection<Inline> arg_44_0 = h.Inlines;
          Run expr_3D = new Run();
          expr_3D.Text=(text);
          arg_44_0.Add(expr_3D);
          h.Click+=(delegate(object s, RoutedEventArgs e)
          {
              clickedCallback.Invoke(h, tag);
          });
          return h;
      }

    public static HyperlinkState GetState(Hyperlink h)
    {
      return (HyperlinkState) Enum.Parse(typeof (HyperlinkState), h.TargetName, false);
    }

    public static void SetState(Hyperlink h, HyperlinkState state, Brush foregroundBrush = null)
    {
      h.TargetName=(state.ToString());
      if (state != HyperlinkState.Accent)
      {
        if (state == HyperlinkState.MatchForeground)
          HyperlinkHelper.SetForegroundStyleForHyperlink(h, foregroundBrush as SolidColorBrush);
        else
          HyperlinkHelper.SetAccentStyleForHyperlink(h);
      }
      else
        HyperlinkHelper.SetAccentStyleForHyperlink(h);
    }

    private static void SetUnderlinedStyleForHyperlink(Hyperlink h, Brush foregroundBrush = null)
    {
      if (foregroundBrush == null)
        foregroundBrush = (Brush) (Application.Current.Resources["PhoneForegroundBrush"] as SolidColorBrush);
      ((TextElement) h).Foreground = foregroundBrush;
      SolidColorBrush solidColorBrush = new SolidColorBrush(((SolidColorBrush) foregroundBrush).Color);
      ((Brush) solidColorBrush).Opacity = 0.67;
      h.MouseOverForeground=((Brush) solidColorBrush);
      ((Inline) h).TextDecorations = TextDecorations.Underline;
      h.MouseOverTextDecorations = TextDecorations.Underline;
    }

    private static void SetAccentStyleForHyperlink(Hyperlink h)
    {
      SolidColorBrush solidColorBrush1 = Application.Current.Resources["PhoneNameBlueBrush"] as SolidColorBrush;
      SolidColorBrush solidColorBrush2 = new SolidColorBrush(solidColorBrush1.Color);
      ((Brush) solidColorBrush2).Opacity = 0.667;
      ((TextElement) h).Foreground = ((Brush) solidColorBrush1);
      h.MouseOverForeground=((Brush) solidColorBrush2);
      h.MouseOverTextDecorations=( null);
      ((Inline) h).TextDecorations=( null);
    }

    private static void SetForegroundStyleForHyperlink(Hyperlink h, SolidColorBrush foregroundBrush)
    {
      if (foregroundBrush == null)
        return;
      SolidColorBrush solidColorBrush1 = new SolidColorBrush(foregroundBrush.Color);
      double num = 0.667;
      ((Brush) solidColorBrush1).Opacity = num;
      SolidColorBrush solidColorBrush2 = solidColorBrush1;
      ((TextElement) h).Foreground = ((Brush) foregroundBrush);
      h.MouseOverForeground=((Brush) solidColorBrush2);
      h.MouseOverTextDecorations=( null);
      ((Inline) h).TextDecorations=( null);
    }
  }
}
