using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VKClient.Common.Utils
{
  public class RectangleHelper
  {
    public static List<Rectangle> CoverByRectangles(Rectangle rect)
    {
      List<Rectangle> rectangleList = new List<Rectangle>();
      if (rect.Height > 2000.0)
      {
        Thickness thickness1 = rect.Margin;
        double num1 = 0.0;
        while (num1 < rect.Height)
        {
          Rectangle rectangle1 = new Rectangle();
          Brush fill = rect.Fill;
          rectangle1.Fill = fill;
          double width = rect.Width;
          rectangle1.Width = width;
          double num2 = Math.Min(2000.0, rect.Height - num1 + 1.0);
          rectangle1.Height = num2;
          double opacity = rect.Opacity;
          rectangle1.Opacity = opacity;
          Thickness thickness2 = thickness1;
          rectangle1.Margin = thickness2;
          int num3 = (int) rect.HorizontalAlignment;
          rectangle1.HorizontalAlignment = (HorizontalAlignment) num3;
          int num4 = (int) rect.VerticalAlignment;
          rectangle1.VerticalAlignment = (VerticalAlignment) num4;
          Rectangle rectangle2 = rectangle1;
          rectangleList.Add(rectangle2);
          thickness1 = new Thickness(thickness1.Left, thickness1.Top + rectangle2.Height - 1.0, thickness1.Right, thickness1.Bottom);
          num1 += rectangle2.Height - 1.0;
        }
      }
      else
        rectangleList.Add(rect);
      return rectangleList;
    }
  }
}
