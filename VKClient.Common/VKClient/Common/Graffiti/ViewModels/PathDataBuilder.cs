using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VKClient.Common.Graffiti.ViewModels
{
  public static class PathDataBuilder
  {
    public static PathFigure CreatePathFigure(Point startPoint)
    {
      return new PathFigure() { StartPoint = startPoint, Segments = new PathSegmentCollection(), IsClosed = false, IsFilled = false };
    }

    public static Path CreatePath(PathFigure pathFigure, double lineStrokeThickness, Brush strokeBrush)
    {
      PathGeometry pathGeometry = new PathGeometry();
      pathGeometry.Figures.Add(pathFigure);
      Path path = new Path();
      path.Data = (Geometry) pathGeometry;
      double num1 = lineStrokeThickness;
      path.StrokeThickness = num1;
      Brush brush = strokeBrush;
      path.Stroke = brush;
      int num2 = 2;
      path.StrokeStartLineCap = (PenLineCap) num2;
      int num3 = 2;
      path.StrokeEndLineCap = (PenLineCap) num3;
      int num4 = 2;
      path.StrokeLineJoin = (PenLineJoin) num4;
      return path;
    }
  }
}
