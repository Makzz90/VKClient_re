using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VKClient.Common.Graffiti.ViewModels
{
  public static class PathDataBuilder
  {
    public static PathFigure CreatePathFigure(Point startPoint)
    {
      PathFigure pathFigure = new PathFigure();
      Point point = startPoint;
      pathFigure.StartPoint = point;
      PathSegmentCollection segmentCollection = new PathSegmentCollection();
      pathFigure.Segments = segmentCollection;
      int num1 = 0;
      pathFigure.IsClosed=(num1 != 0);
      int num2 = 0;
      pathFigure.IsFilled=(num2 != 0);
      return pathFigure;
    }

    public static Path CreatePath(PathFigure pathFigure, double lineStrokeThickness, Brush strokeBrush)
    {
      PathGeometry pathGeometry1 = new PathGeometry();
      ((PresentationFrameworkCollection<PathFigure>) pathGeometry1.Figures).Add(pathFigure);
      Path path = new Path();
      PathGeometry pathGeometry2 = pathGeometry1;
      path.Data=((Geometry) pathGeometry2);
      double num1 = lineStrokeThickness;
      ((Shape) path).StrokeThickness = num1;
      Brush brush = strokeBrush;
      ((Shape) path).Stroke = brush;
      int num2 = 2;
      ((Shape) path).StrokeStartLineCap=((PenLineCap) num2);
      int num3 = 2;
      ((Shape) path).StrokeEndLineCap=((PenLineCap) num3);
      int num4 = 2;
      ((Shape) path).StrokeLineJoin=((PenLineJoin) num4);
      return path;
    }
  }
}
