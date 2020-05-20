using System;
using System.Windows.Media;

namespace VKClient.Common.Utils
{
  public class ColorHelper
  {
    public static Color GetColorFromString(string colorHex)
    {
      return Color.FromArgb(Convert.ToByte(colorHex.Substring(1, 2), 16), Convert.ToByte(colorHex.Substring(3, 2), 16), Convert.ToByte(colorHex.Substring(5, 2), 16), Convert.ToByte(colorHex.Substring(7, 2), 16));
    }

    public static SolidColorBrush GetBrushFromString(string colorHex)
    {
      return new SolidColorBrush(ColorHelper.GetColorFromString(colorHex));
    }
  }
}
