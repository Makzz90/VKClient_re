using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;

namespace VKClient.Common.Graffiti.Cache
{
  [DataContract]
  public class GraffitiCacheData
  {
    private GraffitiCacheDataCurve _currentCurve = new GraffitiCacheDataCurve();

    [DataMember]
    public List<GraffitiCacheDataCurve> Curves { get; set; }

    [DataMember]
    public int SelectedStrokeThickness { get; set; }

    [DataMember]
    public string SelectedColorHex { get; set; }

    public GraffitiCacheData()
    {
      this.Curves = new List<GraffitiCacheDataCurve>();
    }

    public void AddPoint(Point point, bool isLastPoint = false)
    {
      this._currentCurve.AddPoint(point);
      if (!isLastPoint)
        return;
      this.Curves.Add(this._currentCurve);
      this._currentCurve = new GraffitiCacheDataCurve();
    }

    public void RemoveLastCurve()
    {
      if (this.Curves.Count == 0)
        return;
      this.Curves.RemoveAt(this.Curves.Count - 1);
    }

    public void RemoveAllCurves()
    {
      this.Curves.Clear();
    }

    public void SetCurveStrokeThickness(int strokeThickness)
    {
      this._currentCurve.StrokeThickness = strokeThickness;
    }

    public void SetCurveColorHex(string colorHex)
    {
      this._currentCurve.ColorHex = colorHex;
    }
  }
}
