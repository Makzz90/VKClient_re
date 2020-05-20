using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using VKClient.Common.Graffiti.Cache;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class GraffitiDrawService
  {
    private readonly List<Point> _currentPoints = new List<Point>();
    private readonly List<Curve> _curves = new List<Curve>();
    private readonly Curve _currentCurve = new Curve();
    private readonly List<PathFigure> _currentCurvePathFigures = new List<PathFigure>();
    private readonly CurveData _currentCurveData = new CurveData();
    private const int MAX_POINTS_COUNT = 100;
    private const int MIN_POINTS_COUNT = 20;
    private bool _canAddPoints;

    public GraffitiCacheData GraffitiCacheData { get;set; }

    public List<CurveData> CurvesData { get;set; }

    public int StrokeThickness { private get; set; }

    public SolidColorBrush StrokeBrush { private get; set; }

    public bool CanUndo
    {
      get
      {
        if (this._curves.Count <= 0)
          return this._currentPoints.Count > 0;
        return true;
      }
    }

    public Path HandleTouchPoint(Point point, bool isLastPoint = false)
    {
      if (this._currentPoints.Count > 100)
      {
        this._canAddPoints = false;
        while (this._currentPoints.Count > 20)
          this._currentPoints.RemoveAt(0);
      }
      this._currentPoints.Add(point);
      this._currentCurveData.Points.Add(point);
      Path path = (Path) null;
      if (!this._canAddPoints)
      {
        PathFigure pathFigure;
        this.InitNewPath(out path, out pathFigure);
        this._currentCurveData.StrokeBrush = (Brush) this.StrokeBrush;
        this._currentCurveData.StrokeThickness = this.StrokeThickness;
        this.GraffitiCacheData.SetCurveStrokeThickness(this.StrokeThickness);
        this.GraffitiCacheData.SetCurveColorHex(this.StrokeBrush.Color.ToString());
        this._currentCurve.Add(path);
        this._currentCurvePathFigures.Add(pathFigure);
        this._canAddPoints = true;
      }
      this.GraffitiCacheData.AddPoint(point, isLastPoint);
      this._currentCurvePathFigures.Last<PathFigure>().Segments = GraffitiDrawService.GetSegments((IReadOnlyList<Point>) this._currentPoints);
      if (isLastPoint)
        this.Checkpoint();
      return path;
    }

    private void InitNewPath(out Path path, out PathFigure pathFigure)
    {
      pathFigure = PathDataBuilder.CreatePathFigure(this._currentPoints[0]);
      path = PathDataBuilder.CreatePath(pathFigure, (double) this.StrokeThickness, (Brush) this.StrokeBrush);
    }

    public static PathSegmentCollection GetSegments(IReadOnlyList<Point> controlPoints)
    {
      if (controlPoints.Count == 0)
        return (PathSegmentCollection) null;
      PathSegmentCollection segmentCollection = new PathSegmentCollection();
      Point point1 = controlPoints[0];
      double x1 = point1.X;
      point1 = controlPoints[0];
      double y1 = point1.Y;
      if (controlPoints.Count <= 3)
      {
        LineSegment lineSegment = new LineSegment() { Point = new Point(x1 + 1.0, y1) };
        segmentCollection.Add((PathSegment) lineSegment);
      }
      else
      {
        for (int index = 1; index < controlPoints.Count; ++index)
        {
          point1 = controlPoints[index - 1];
          double x2 = point1.X;
          point1 = controlPoints[index - 1];
          double y2 = point1.Y;
          point1 = controlPoints[index];
          double x3 = point1.X;
          point1 = controlPoints[index];
          double y3 = point1.Y;
          if (Math.Sqrt(Math.Pow(x3 - x2, 2.0) + Math.Pow(y3 - y2, 2.0)) < 2.0)
          {
            LineSegment lineSegment = new LineSegment() { Point = new Point(x3, y3) };
            segmentCollection.Add((PathSegment) lineSegment);
          }
          else
          {
            PolyQuadraticBezierSegment quadraticBezierSegment1 = new PolyQuadraticBezierSegment();
            PointCollection pointCollection = new PointCollection();
            Point point2 = new Point(x2, y2);
            pointCollection.Add(point2);
            Point point3 = new Point((x2 + x3) / 2.0, (y2 + y3) / 2.0);
            pointCollection.Add(point3);
            quadraticBezierSegment1.Points = pointCollection;
            PolyQuadraticBezierSegment quadraticBezierSegment2 = quadraticBezierSegment1;
            segmentCollection.Add((PathSegment) quadraticBezierSegment2);
          }
        }
      }
      return segmentCollection;
    }

    private void Checkpoint()
    {
      this._canAddPoints = false;
      this._currentPoints.Clear();
      this.CurvesData.Add(CurveData.BuildFrom(this._currentCurveData));
      this._currentCurveData.Points.Clear();
      this._curves.Add(new Curve((IEnumerable<Path>) this._currentCurve.ToList<Path>()));
      this._currentCurve.Clear();
    }

    public Curve Undo()
    {
      Curve curve = this._curves.LastOrDefault<Curve>();
      if (curve == null)
        return (Curve) null;
      CurveData curveData = this.CurvesData.LastOrDefault<CurveData>();
      if (curveData != null)
        this.CurvesData.Remove(curveData);
      this._curves.RemoveAt(this._curves.Count - 1);
      this.GraffitiCacheData.RemoveLastCurve();
      return curve;
    }

    public void Clear()
    {
      this._canAddPoints = false;
      this._currentPoints.Clear();
      this.CurvesData.Clear();
      this._curves.Clear();
      this.GraffitiCacheData.RemoveAllCurves();
    }

      public GraffitiDrawService()
		{
			this.GraffitiCacheData = new GraffitiCacheData();
			this.CurvesData = new List<CurveData>();
		}
  }
}
