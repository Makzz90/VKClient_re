using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Audio.Base.Extensions
{
  public static class TextBlockExtensions
  {
    private static readonly Dictionary<string, string> _trimmedDict = new Dictionary<string, string>();

    public static void CorrectText(this TextBlock textName, double maxWidth)
    {
      if (string.IsNullOrWhiteSpace(textName.Text))
        return;
      string key = string.Format("{0}-{1}", textName.Text, maxWidth);
      if (TextBlockExtensions._trimmedDict.ContainsKey(key))
      {
        textName.Text=(TextBlockExtensions._trimmedDict[key]);
      }
      else
      {
        if (((FrameworkElement) textName).ActualWidth <= maxWidth)
          return;
        double num = ((FrameworkElement) textName).ActualWidth / (double) textName.Text.Length;
        int int32 = Convert.ToInt32(maxWidth / num);
        if (int32 >= textName.Text.Length || int32 <= 4)
          return;
        textName.Text = (textName.Text.Substring(0, int32).Trim() + "...");
        for (int index = 0; ((FrameworkElement) textName).ActualWidth > maxWidth && index < 10; ++index)
            textName.Text = (textName.Text.Substring(0, textName.Text.Length - 4).Trim() + "...");
        TextBlockExtensions._trimmedDict[key] = textName.Text;
      }
    }
  }
}
