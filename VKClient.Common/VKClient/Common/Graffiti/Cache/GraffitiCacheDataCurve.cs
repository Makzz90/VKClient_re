using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace VKClient.Common.Graffiti.Cache
{
  [DataContract]
  public class GraffitiCacheDataCurve
  {
    [DataMember]
    public List<GraffitiCacheDataPoint> Points { get; set; }

    [DataMember]
    public int StrokeThickness { get; set; }

    [DataMember]
    public string ColorHex { get; set; }

    public GraffitiCacheDataCurve()
    {
      this.Points = new List<GraffitiCacheDataPoint>();
    }

    public void AddPoint(Point point)
    {
      this.Points.Add(new GraffitiCacheDataPoint(point.X, point.Y));
    }

    public List<Point> GetPoints()
    {
      return this.Points.Select<GraffitiCacheDataPoint, Point>((Func<GraffitiCacheDataPoint, Point>) (point => new Point(point.X, point.Y))).ToList<Point>();
    }
  }
}
