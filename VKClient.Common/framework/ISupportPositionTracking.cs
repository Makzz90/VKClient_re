using System.Windows;

namespace VKClient.Common.Framework
{
  public interface ISupportPositionTracking
  {
    void TrackPositionChanged(Rect bounds, double offset);
  }
}
