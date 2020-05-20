using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Utils;

namespace VKClient.Common.UC
{
  public abstract class NewsfeedPromoUC : UserControl
  {
    protected abstract FrameworkElement GridCutArea { get; }

    protected abstract Grid GridBackground { get; }

    protected abstract FrameworkElement GridMessage { get; }

    protected abstract Polygon PolygonTriangle { get; }

    protected NewsfeedPromoUC()
    {
      //base.\u002Ector();
    }

    public void SetCutArea(double marginLeft, double width)
    {
      double num1 = Math.Round(width + 32.0);
      double num2 = (double) ScaleFactor.GetRealScaleFactor() / 100.0;
      int divident;
      int divisor;
      ScaleFactor.GetScaleFactorLowestFraction(out divident, out divisor, true);
      double num3 = Math.Round(num1 / (double) divisor) * (double) divident / num2;
      marginLeft = Math.Round(marginLeft) - 16.0;
      this.GridCutArea.Width = num3;
      ((PresentationFrameworkCollection<ColumnDefinition>) this.GridBackground.ColumnDefinitions)[0].Width=(new GridLength(marginLeft));
      this.UpdateTrianglePosition(marginLeft + width / 2.0);
    }

    private void UpdateTrianglePosition(double centerX)
    {
      Thickness margin = this.GridMessage.Margin;
      // ISSUE: explicit reference operation
      double top = ((Thickness) @margin).Top;
      Polygon polygonTriangle = this.PolygonTriangle;
      PointCollection pointCollection = new PointCollection();
      Point point1 = new Point(centerX - 12.0, top);
      ((PresentationFrameworkCollection<Point>) pointCollection).Add(point1);
      Point point2 = new Point(centerX, top - 12.0);
      ((PresentationFrameworkCollection<Point>) pointCollection).Add(point2);
      Point point3 = new Point(centerX + 12.0, top);
      ((PresentationFrameworkCollection<Point>) pointCollection).Add(point3);
      polygonTriangle.Points = pointCollection;
    }
  }
}
