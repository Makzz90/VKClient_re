using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.ImageViewer
{
  public static class RectangleUtils
  {
    public static Rect CalculateRelative(Size parentSize, Rect relativeRect)
    {
      Rect rect =  new Rect();
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      rect.X = (((Size)@parentSize).Width * ((Rect)@relativeRect).X);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      rect.Y = (((Size)@parentSize).Height * ((Rect)@relativeRect).Y);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      rect.Width = (((Size)@parentSize).Width * ((Rect)@relativeRect).Width);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      rect.Height = (((Size)@parentSize).Height * ((Rect)@relativeRect).Height);
      return rect;
    }

    public static Size GetRectSize(Rect rect)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return new Size(rect.Width, rect.Height);
    }

    public static RectSides GetViolatedConstraints(Rect parentRect, Rect childRect)
    {
      RectSides rectSides = RectSides.None;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @childRect).Left > ((Rect) @parentRect).Left && ((Rect) @childRect).Right > ((Rect) @parentRect).Right)
        rectSides |= RectSides.Left;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @childRect).Top > ((Rect) @parentRect).Top && ((Rect) @childRect).Bottom > ((Rect) @parentRect).Bottom)
        rectSides |= RectSides.Top;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @childRect).Right < ((Rect) @parentRect).Right && ((Rect) @childRect).Left < ((Rect) @parentRect).Left)
        rectSides |= RectSides.Right;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @childRect).Bottom < ((Rect) @parentRect).Bottom && ((Rect) @childRect).Top < ((Rect) @parentRect).Top)
        rectSides |= RectSides.Bottom;
      return rectSides;
    }

    public static Rect ResizeToFit(Rect parentRect, Size childSize)
    {
      Rect fit = RectangleUtils.ResizeToFit(RectangleUtils.GetSize(parentRect), childSize);
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      // ISSUE: explicit reference operation
      double num1 = fit.X + parentRect.X;
      fit.X = num1;
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      // ISSUE: explicit reference operation
      double num2 = fit.Y + parentRect.Y;
      fit.Y = num2;
      return fit;
    }

    public static Rect ResizeToFitIfNotContained(Size parentSize, Size childSize)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Size) @childSize).Width > ((Size) @parentSize).Width || ((Size) @childSize).Height > ((Size) @parentSize).Height)
        return RectangleUtils.ResizeToFit(parentSize, childSize);
      return new Rect( new Point(), childSize);
    }

    public static Rect ResizeToFit(Size parentSize, Size childSize)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Size) @parentSize).Height == 0.0 || ((Size) @parentSize).Width == 0.0 || (((Size) @childSize).Width == 0.0 || ((Size) @childSize).Height == 0.0))
        return  new Rect();
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      double num1 = ((Size) @parentSize).Width / ((Size) @parentSize).Height;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      double num2 = ((Size) @childSize).Width / ((Size) @childSize).Height;
      Rect rect = new Rect();
      double num3 = num2;
      if (num1 < num3)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num4 = ((Size) @parentSize).Width / ((Size) @childSize).Width;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Width = (((Size)@parentSize).Width);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Height = (((Size)@childSize).Height * num4);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Y = ((((Size)@parentSize).Height - ((Rect)@rect).Height) / 2.0);
      }
      else
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num4 = ((Size) @parentSize).Height / ((Size) @childSize).Height;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Height = (((Size)@parentSize).Height);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Width = (((Size)@childSize).Width * num4);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.X = ((((Size)@parentSize).Width - ((Rect)@rect).Width) / 2.0);
      }
      return rect;
    }

    public static Size GetSize(Rect rect)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return new Size(rect.Width, rect.Height);
    }

    public static Rect ResizeToFill(Rect parentRect, Size childSize)
    {
      Rect fill = RectangleUtils.ResizeToFill(RectangleUtils.GetSize(parentRect), childSize);
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      // ISSUE: explicit reference operation
      double num1 = fill.X + ((Rect)@parentRect).X;
      fill.X = num1;
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      // ISSUE: explicit reference operation
      double num2 = fill.Y + ((Rect)@parentRect).Y;
      fill.Y = num2;
      return fill;
    }

    public static Rect ResizeToFill(Size parentSize, Size childSize)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Size) @parentSize).Height == 0.0 || ((Size) @parentSize).Width == 0.0 || (((Size) @childSize).Width == 0.0 || ((Size) @childSize).Height == 0.0))
          return new Rect();
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      double num1 = ((Size) @parentSize).Width / ((Size) @parentSize).Height;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      double num2 = ((Size) @childSize).Width / ((Size) @childSize).Height;
      Rect rect = new Rect();
      double num3 = num2;
      if (num1 < num3)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num4 = ((Size) @parentSize).Height / ((Size) @childSize).Height;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Height=(((Size) @parentSize).Height);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Width=(((Size) @childSize).Width * num4);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.X=((((Size) @parentSize).Width - rect.Width) / 2.0);
      }
      else
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        double num4 = ((Size) @parentSize).Width / ((Size) @childSize).Width;
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Width=(((Size) @parentSize).Width);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Height=(((Size) @childSize).Height * num4);
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        rect.Y=((((Size) @parentSize).Height - rect.Height) / 2.0);
      }
      return rect;
    }

    public static Rect Rotate90(Rect source)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return new Rect(((Rect) @source).X + (((Rect) @source).Width - ((Rect) @source).Height) / 2.0, ((Rect) @source).Y + (((Rect) @source).Height - ((Rect) @source).Width) / 2.0, ((Rect) @source).Height, ((Rect) @source).Width);
    }

    public static CompositeTransform TransformRect(Rect source, Rect target, bool inSourceCenterCoord = false)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @source).Width == 0.0 || ((Rect) @source).Height == 0.0 || (((Rect) @target).Width == 0.0 || ((Rect) @target).Height == 0.0))
        return new CompositeTransform();
      if (inSourceCenterCoord)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        return RectangleUtils.TransformRect(new Rect(((Rect) @source).X - ((Rect) @source).Width / 2.0, ((Rect) @source).Y - ((Rect) @source).Height / 2.0, ((Rect) @source).Width, ((Rect) @source).Height), new Rect(((Rect) @target).X - ((Rect) @source).Width / 2.0, ((Rect) @target).Y - ((Rect) @source).Height / 2.0, ((Rect) @target).Width, ((Rect) @target).Height), false);
      }
      CompositeTransform compositeTransform = new CompositeTransform();
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      compositeTransform.ScaleX=(((Rect) @target).Width / ((Rect) @source).Width);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      compositeTransform.ScaleY=(((Rect) @target).Height / ((Rect) @source).Height);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      compositeTransform.TranslateX=(((Rect) @target).X - ((Rect) @source).X * compositeTransform.ScaleX);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      compositeTransform.TranslateY=(((Rect) @target).Y - ((Rect) @source).Y * compositeTransform.ScaleY);
      return compositeTransform;
    }

    public static Rect AlignRects(Rect parentRect, Rect childRect, bool fill)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      bool flag1 = Math.Abs(((Rect) @childRect).Bottom - ((Rect) @parentRect).Bottom) < Math.Abs(((Rect) @childRect).Top - ((Rect) @parentRect).Top);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      bool flag2 = Math.Abs(((Rect) @childRect).Left - ((Rect) @parentRect).Left) < Math.Abs(((Rect) @childRect).Right - ((Rect) @parentRect).Right);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      double num1 = Math.Max(((Rect) @parentRect).Width / ((Rect) @childRect).Width, ((Rect) @parentRect).Height / ((Rect) @childRect).Height);
      if (num1 > 1.0 & fill)
      {
        // ISSUE: explicit reference operation
        // ISSUE: variable of a reference type
          double num2 = childRect.Width * num1;
          childRect.Width = num2;
        // ISSUE: explicit reference operation
        // ISSUE: variable of a reference type
          double num3 = childRect.Height * num1;
          childRect.Height = num3;
      }
      double num4 = 0.0;
      double num5 = 0.0;
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @childRect).Top > ((Rect) @parentRect).Top ^ ((Rect) @childRect).Bottom < ((Rect) @parentRect).Bottom)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        num5 = !flag1 ? ((Rect) @parentRect).Top - ((Rect) @childRect).Top : ((Rect) @parentRect).Bottom - ((Rect) @childRect).Bottom;
      }
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      if (((Rect) @childRect).Left > ((Rect) @parentRect).Left ^ ((Rect) @childRect).Right < ((Rect) @parentRect).Right)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        num4 = !flag2 ? ((Rect) @parentRect).Right - ((Rect) @childRect).Right : ((Rect) @parentRect).Left - ((Rect) @childRect).Left;
      }
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      double num6 = childRect.X + num4;
      childRect.X = num6;
      // ISSUE: explicit reference operation
      // ISSUE: variable of a reference type
      double num7 = childRect.Y + num5;
      childRect.Y = num7;
      if (!fill)
      {
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        if (((Rect) @childRect).Height < ((Rect) @parentRect).Height && (((Rect) @childRect).Top > ((Rect) @parentRect).Top || ((Rect) @childRect).Bottom < ((Rect) @parentRect).Bottom))
        {
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
            childRect.Y = (((Rect)@parentRect).Y + (((Rect)@parentRect).Height - ((Rect)@childRect).Height) / 2.0);
        }
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        // ISSUE: explicit reference operation
        if (((Rect) @childRect).Width < ((Rect) @parentRect).Width && (((Rect) @childRect).Left > ((Rect) @parentRect).Left || ((Rect) @childRect).Right < ((Rect) @parentRect).Right))
        {
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
            childRect.X = (((Rect)@parentRect).Y + (((Rect)@parentRect).Width - ((Rect)@childRect).Width) / 2.0);
        }
      }
      return childRect;
    }

    private static List<Point> GetPoints(Rect rect)
    {
      List<Point> pointList = new List<Point>();
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      Point point1 = new Point(rect.Left, rect.Top);
      pointList.Add(point1);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      Point point2 = new Point(rect.Left + rect.Width, rect.Top);
      pointList.Add(point2);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      Point point3 = new Point(rect.Left, rect.Top + rect.Height);
      pointList.Add(point3);
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      Point point4 = new Point(rect.Left + rect.Width, rect.Top + rect.Height);
      pointList.Add(point4);
      return pointList;
    }

    private static double GetDistance(Point p1, Point p2)
    {
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      // ISSUE: explicit reference operation
      return Math.Sqrt((((Point) @p1).X - ((Point) @p2).X) * (((Point) @p1).X - ((Point) @p2).X) + (((Point) @p1).Y - ((Point) @p2).Y) * (((Point) @p1).Y - ((Point) @p2).Y));
    }
  }
}
