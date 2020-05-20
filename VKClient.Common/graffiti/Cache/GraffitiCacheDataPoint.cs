using System.Runtime.Serialization;

namespace VKClient.Common.Graffiti.Cache
{
  [DataContract]
  public class GraffitiCacheDataPoint
  {
    [DataMember]
    public double X { get; set; }

    [DataMember]
    public double Y { get; set; }

    public GraffitiCacheDataPoint()
    {
    }

    public GraffitiCacheDataPoint(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }
  }
}
