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
      PathSegmentCollection segments1 = pathFigure.Segments;
      LineSegment lineSegment1 = new LineSegment();
      Point point1 = p2;
      lineSegment1.Point = point1;
      ((PresentationFrameworkCollection<PathSegment>) segments1).Add((PathSegment) lineSegment1);
      PathSegmentCollection segments2 = pathFigure.Segments;
      LineSegment lineSegment2 = new LineSegment();
      Point point2 = p3;
      lineSegment2.Point = point2;
      ((PresentationFrameworkCollection<PathSegment>) segments2).Add((PathSegment) lineSegment2);
      PathGeometry pathGeometry = new PathGeometry();
      PathFigureCollection figureCollection = new PathFigureCollection();
      pathGeometry.Figures = figureCollection;
      ((PresentationFrameworkCollection<PathFigure>) pathGeometry.Figures).Add(pathFigure);
      return (Geometry) pathGeometry;
    }
  }
}
