using System;

namespace Microsoft.Phone.Applications.Common
{
  public class AccelerometerHelperReadingEventArgs : EventArgs
  {
    public Simple3DVector RawAcceleration { get; set; }

    public Simple3DVector OptimallyFilteredAcceleration { get; set; }

    public Simple3DVector LowPassFilteredAcceleration { get; set; }

    public Simple3DVector AverageAcceleration { get; set; }
  }
}
