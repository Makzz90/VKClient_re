using System.Windows;
using System.Windows.Shapes;

namespace VKClient.Common.Framework
{
  public class RectangePlaceholder
  {
    public static Rectangle CreateImagePlaceholder(double width, double height)
    {
      Rectangle rectangle = new Rectangle();
      double num1 = width;
      ((FrameworkElement) rectangle).Width = num1;
      double num2 = height;
      ((FrameworkElement) rectangle).Height = num2;
      Style style = Application.Current.Resources["PhotoPlaceholderRectangle"] as Style;
      ((FrameworkElement) rectangle).Style = style;
      return rectangle;
    }
  }
}
