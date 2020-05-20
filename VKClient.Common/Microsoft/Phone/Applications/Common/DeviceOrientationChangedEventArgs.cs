using System;

namespace Microsoft.Phone.Applications.Common
{
  public class DeviceOrientationChangedEventArgs : EventArgs
  {
    public DeviceOrientation CurrentOrientation { get; set; }

    public DeviceOrientation PreviousOrientation { get; set; }
  }
}
