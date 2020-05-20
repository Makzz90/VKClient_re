using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace VKClient.Common.Graffiti.ViewModels
{
  public class CurveData
  {
      public List<Point> Points { get; set; }

    public Brush StrokeBrush { get; set; }

    public int StrokeThickness { get; set; }

    public CurveData()
    {
      this.Points = new List<Point>();
    }

    public static CurveData BuildFrom(CurveData data)
    {
      CurveData curveData = new CurveData();
      curveData.StrokeBrush = data.StrokeBrush;
      curveData.StrokeThickness = data.StrokeThickness;
      curveData.Points.AddRange((IEnumerable<Point>) data.Points.ToList<Point>());
      return curveData;
    }
  }
}
