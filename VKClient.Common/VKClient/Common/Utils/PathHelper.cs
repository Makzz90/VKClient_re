using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Utils
{
  public class PathHelper
  {
    public static Geometry CreateTriangleGeometry(Point p1, Point p2, Point p3)
    {
      PathFigure pathFigure = new PathFigure();
      pathFigure.StartPoint = p1;
      pathFigure.Segments.Add((PathSegment) new LineSegment()
      {
        Point = p2
      });
      pathFigure.Segments.Add((PathSegment) new LineSegment()
      {
        Point = p3
      });
      PathGeometry pathGeometry = new PathGeometry();
      pathGeometry.Figures = new PathFigureCollection();
      pathGeometry.Figures.Add(pathFigure);
      return (Geometry) pathGeometry;
    }
  }
}
