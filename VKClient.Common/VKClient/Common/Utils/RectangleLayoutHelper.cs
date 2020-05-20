using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace VKClient.Common.Utils
{
  public class RectangleLayoutHelper
  {
    public static List<Rect> CreateLayout(Size parentRect, List<Size> childrenRects, double marginBetween)
    {
      List<ThumbAttachment> thumbAttachments = RectangleLayoutHelper.ConvertSizesToThumbAttachments(childrenRects);
      ThumbnailLayoutManager.ProcessThumbnails(parentRect.Width, parentRect.Height, thumbAttachments, marginBetween);
      return RectangleLayoutHelper.ConvertProcessedThumbsToRects(thumbAttachments, marginBetween, parentRect.Width);
    }

    private static List<Rect> ConvertProcessedThumbsToRects(List<ThumbAttachment> thumbs, double marginBetween, double width)
    {
      List<Rect> rectList = new List<Rect>(thumbs.Count);
      double y = 0.0;
      double widthOfRow = RectangleLayoutHelper.CalculateWidthOfRow(thumbs, marginBetween);
      double num = width / 2.0 - widthOfRow / 2.0;
      double x = num;
      for (int index = 0; index < thumbs.Count; ++index)
      {
        ThumbAttachment thumbAttachment = thumbs[index];
        rectList.Add(new Rect(x, y, thumbAttachment.CalcWidth, thumbAttachment.CalcHeight));
        if (!thumbAttachment.LastColumn && !thumbAttachment.LastRow)
          x += thumbAttachment.CalcWidth + marginBetween;
        else if (thumbAttachment.LastRow)
          y += thumbAttachment.CalcHeight + marginBetween;
        else if (thumbAttachment.LastColumn)
        {
          x = num;
          y += thumbAttachment.CalcHeight + marginBetween;
        }
      }
      return rectList;
    }

    private static double CalculateWidthOfRow(List<ThumbAttachment> thumbs, double marginBetween)
    {
      double num = 0.0;
      foreach (ThumbAttachment thumb in thumbs)
      {
        num += thumb.CalcWidth;
        num += marginBetween;
        if (!thumb.LastRow)
        {
          if (thumb.LastColumn)
            break;
        }
        else
          break;
      }
      if (num > 0.0)
        num -= marginBetween;
      return num;
    }

    private static List<ThumbAttachment> ConvertSizesToThumbAttachments(List<Size> childrenRects)
    {
      return childrenRects.Select<Size, ThumbAttachment>((Func<Size, ThumbAttachment>) (r => new ThumbAttachment()
      {
        Height = r.Height > 0.0 ? r.Height : 100.0,
        Width = r.Width > 0.0 ? r.Width : 100.0
      })).ToList<ThumbAttachment>();
    }
  }
}
