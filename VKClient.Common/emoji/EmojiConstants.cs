using System;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
  public static class EmojiConstants
  {
    public static int ColumnsCountVerticalOrientation
    {
      get
      {
        return !ScaleFactor.IsFullHD() ? 8 : 9;
      }
    }

    public static int ColumnsCountHorizontalOrientation
    {
      get
      {
        return !ScaleFactor.IsFullHD() ? 12 : 13;
      }
    }

    public static double EmojiWidth
    {
      get
      {
        return ScaleFactor.IsFullHD() ? 24.0 : 32.0;
      }
    }

    public static double EmojiHeight
    {
      get
      {
        return ScaleFactor.IsFullHD() ? 24.0 : 32.0;
      }
    }

    public static int VerticalSpriteRowsCount
    {
      get
      {
        return !ScaleFactor.IsFullHD() ? 5 : 6;
      }
    }

    public static int HorizontalSpriteRowsCount
    {
      get
      {
        return !ScaleFactor.IsFullHD() ? 4 : 5;
      }
    }

    public static double VerticalSpriteWidth
    {
      get
      {
        return 480.0;
      }
    }

    public static double VerticalSpriteHeight
    {
      get
      {
        return 328.0;
      }
    }

    public static double HorizontalSpriteHeight
    {
      get
      {
        return 246.0;
      }
    }

    public static int EmojiWidthInPixels
    {
      get
      {
        return Convert.ToInt32(EmojiConstants.EmojiWidth * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static int EmojiHeightInPixels
    {
      get
      {
        return Convert.ToInt32(EmojiConstants.EmojiHeight * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static int VerticalSpriteWidthInPixels
    {
      get
      {
        return Convert.ToInt32(EmojiConstants.VerticalSpriteWidth * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static int VerticalSpriteHeightInPixels
    {
      get
      {
        return Convert.ToInt32(EmojiConstants.VerticalSpriteHeight * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static double HorizontalSpriteWidth
    {
      get
      {
        return ScaleFactor.GetRealScaleFactor() == 150 || ScaleFactor.GetRealScaleFactor() == 225 ? 818.0 : 800.0;
      }
    }

    public static int HorizontalSpriteWidthInPixels
    {
      get
      {
        return Convert.ToInt32(EmojiConstants.HorizontalSpriteWidth * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static int HorizontalSpriteHeightInPixels
    {
      get
      {
        return Convert.ToInt32(EmojiConstants.HorizontalSpriteHeight * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }
  }
}
