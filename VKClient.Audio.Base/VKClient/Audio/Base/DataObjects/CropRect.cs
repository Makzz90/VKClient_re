using System.Windows;

namespace VKClient.Audio.Base.DataObjects
{
  public class CropRect
  {
    public double x { get; set; }

    public double y { get; set; }

    public double x2 { get; set; }

    public double y2 { get; set; }

    public Rect GetCroppingRectangle(double width, double height)
    {
      double num1 = this.x * width / 100.0;
      double num2 = this.x2 * width / 100.0;
      double num3 = this.y * height / 100.0;
      double num4 = this.y2 * height / 100.0;
      double num5 = num2 - num1;
      double num6 = num3;
      double num7 = num4 - num6;
      return new Rect((double) (int) num1, (double) (int) num3, (double) (int) num5, (double) (int) num7);
    }
  }
}
