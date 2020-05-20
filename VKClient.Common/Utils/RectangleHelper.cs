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
      if (((FrameworkElement) rect).Height > 2000.0)
      {
        Thickness margin = ((FrameworkElement) rect).Margin;
        double num1 = 0.0;
        while (num1 < ((FrameworkElement) rect).Height)
        {
          Rectangle rectangle1 = new Rectangle();
          Brush fill = ((Shape) rect).Fill;
          ((Shape) rectangle1).Fill = fill;
          double width = ((FrameworkElement) rect).Width;
          ((FrameworkElement) rectangle1).Width = width;
          double num2 = Math.Min(2000.0, ((FrameworkElement) rect).Height - num1 + 1.0);
          ((FrameworkElement) rectangle1).Height = num2;
          double opacity = ((UIElement) rect).Opacity;
          ((UIElement) rectangle1).Opacity = opacity;
          Thickness thickness = margin;
          ((FrameworkElement) rectangle1).Margin = thickness;
          HorizontalAlignment horizontalAlignment = ((FrameworkElement) rect).HorizontalAlignment;
          ((FrameworkElement) rectangle1).HorizontalAlignment = horizontalAlignment;
          VerticalAlignment verticalAlignment = ((FrameworkElement) rect).VerticalAlignment;
          ((FrameworkElement) rectangle1).VerticalAlignment = verticalAlignment;
          Rectangle rectangle2 = rectangle1;
          rectangleList.Add(rectangle2);
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          // ISSUE: explicit reference operation
          margin=new Thickness(margin.Left, margin.Top + ((FrameworkElement) rectangle2).Height - 1.0, margin.Right, margin.Bottom);
          num1 += ((FrameworkElement) rectangle2).Height - 1.0;
        }
      }
      else
        rectangleList.Add(rect);
      return rectangleList;
    }
  }
}
