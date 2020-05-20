using System;
using VKClient.Common.Utils;

namespace VKClient.Common.Emoji
{
  public static class StickersConstants
  {
    public static int ColumnsCountVerticalOrientation
    {
      get
      {
        return 4;
      }
    }

    public static int ColumnsCountHorizontalOrientation
    {
      get
      {
        return 5;
      }
    }

    public static double StickerWidth
    {
      get
      {
        return 100.0;
      }
    }

    public static int StickerWidthInPixels
    {
      get
      {
        return Convert.ToInt32(StickersConstants.StickerWidth * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static double StickerHeight
    {
      get
      {
        return 105.0;
      }
    }

    public static int StickerHeightInPixels
    {
      get
      {
        return Convert.ToInt32(StickersConstants.StickerHeight * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static int VerticalSpriteRowsCount
    {
      get
      {
        return 1;
      }
    }

    public static int HorizontalSpriteRowsCount
    {
      get
      {
        return 1;
      }
    }

    public static double VerticalSpriteWidth
    {
      get
      {
        return 480.0;
      }
    }

    public static int VerticalSpriteWidthInPixels
    {
      get
      {
        return Convert.ToInt32(StickersConstants.VerticalSpriteWidth * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static double VerticalSpriteHeight
    {
      get
      {
        return 132.0;
      }
    }

    public static int VerticalSpriteHeightInPixels
    {
      get
      {
        return Convert.ToInt32(StickersConstants.VerticalSpriteHeight * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
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
        return Convert.ToInt32(StickersConstants.HorizontalSpriteWidth * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }

    public static double HorizontalSpriteHeight
    {
      get
      {
        return 132.0;
      }
    }

    public static int HorizontalSpriteHeightInPixels
    {
      get
      {
        return Convert.ToInt32(StickersConstants.HorizontalSpriteHeight * (double) ScaleFactor.GetRealScaleFactor() / 100.0);
      }
    }
  }
}
