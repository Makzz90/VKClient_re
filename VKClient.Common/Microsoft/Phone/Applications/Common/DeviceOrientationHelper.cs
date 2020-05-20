using System;
using System.Collections.Generic;

namespace Microsoft.Phone.Applications.Common
{
  public sealed class DeviceOrientationHelper
  {
    private static object _syncRoot = new object();
    private static volatile DeviceOrientationHelper _singletonInstance;
    private static Dictionary<DeviceOrientation, DeviceOrientationInfo> _deviceOrientationInfoList;
    private const double tiltAccelerationThreshold = 0.8;
    private DeviceOrientation _currentOrientation;
    private DeviceOrientation _previousOrientation;

    public static DeviceOrientationHelper Instance
    {
      get
      {
        if (DeviceOrientationHelper._singletonInstance == null)
        {
          lock (DeviceOrientationHelper._syncRoot)
          {
            if (DeviceOrientationHelper._singletonInstance == null)
              DeviceOrientationHelper._singletonInstance = new DeviceOrientationHelper();
          }
        }
        return DeviceOrientationHelper._singletonInstance;
      }
    }

    public DeviceOrientation CurrentOrientation
    {
      get
      {
        return this._currentOrientation;
      }
    }

    public event EventHandler<DeviceOrientationChangedEventArgs> OrientationChanged;

    private DeviceOrientationHelper()
    {
      if (DeviceOrientationHelper._deviceOrientationInfoList == null)
      {
        DeviceOrientationHelper._deviceOrientationInfoList = new Dictionary<DeviceOrientation, DeviceOrientationInfo>();
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.Unknown, new DeviceOrientationInfo(0.0, 0, new Simple3DVector(0.0, 0.0, 0.0)));
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.ScreenSideUp, new DeviceOrientationInfo(0.0, 1, new Simple3DVector(0.0, 0.0, -1.0)));
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.ScreenSideDown, new DeviceOrientationInfo(0.0, 1, new Simple3DVector(0.0, 0.0, 1.0)));
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.LandscapeRight, new DeviceOrientationInfo(-90.0, -1, new Simple3DVector(-1.0, 0.0, 0.0)));
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.LandscapeLeft, new DeviceOrientationInfo(90.0, 1, new Simple3DVector(1.0, 0.0, 0.0)));
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.PortraitRightSideUp, new DeviceOrientationInfo(0.0, -1, new Simple3DVector(0.0, -1.0, 0.0)));
        DeviceOrientationHelper._deviceOrientationInfoList.Add(DeviceOrientation.PortraitUpSideDown, new DeviceOrientationInfo(-180.0, 1, new Simple3DVector(0.0, 1.0, 0.0)));
      }
      AccelerometerHelper.Instance.ReadingChanged += new EventHandler<AccelerometerHelperReadingEventArgs>(this.accelerometerHelper_ReadingChanged);
    }

    public static DeviceOrientationInfo GetDeviceOrientationInfo(DeviceOrientation deviceOrientation)
    {
      return DeviceOrientationHelper._deviceOrientationInfoList[deviceOrientation];
    }

    public static bool IsLandscape(DeviceOrientation deviceOrientation)
    {
      return DeviceOrientationHelper.GetDeviceOrientationInfo(deviceOrientation).NormalGravityVector.X != 0.0;
    }

    public static bool IsPortrait(DeviceOrientation deviceOrientation)
    {
      return DeviceOrientationHelper.GetDeviceOrientationInfo(deviceOrientation).NormalGravityVector.Y != 0.0;
    }

    public static bool IsFlat(DeviceOrientation deviceOrientation)
    {
      return DeviceOrientationHelper.GetDeviceOrientationInfo(deviceOrientation).NormalGravityVector.Z != 0.0;
    }

    private void accelerometerHelper_ReadingChanged(object sender, AccelerometerHelperReadingEventArgs e)
    {
      this.CheckOrientation(e.LowPassFilteredAcceleration);
    }

    private void CheckOrientation(Simple3DVector filteredAcceleration)
    {
      DeviceOrientation deviceOrientation1 = DeviceOrientation.Unknown;
      double x = filteredAcceleration.X;
      double y = filteredAcceleration.Y;
      double z = filteredAcceleration.Z;
      double num1 = Math.Sqrt(x * x + y * y + z * z);
      double num2 = x / num1;
      double num3 = y / num1;
      double num4 = z / num1;
      if (this._currentOrientation == DeviceOrientation.Unknown)
        deviceOrientation1 = num4 >= 0.0 ? DeviceOrientation.ScreenSideDown : DeviceOrientation.ScreenSideUp;
      if (num3 < -0.8)
        deviceOrientation1 = DeviceOrientation.PortraitRightSideUp;
      else if (num3 > 0.8)
        deviceOrientation1 = DeviceOrientation.PortraitUpSideDown;
      else if (num2 < -0.8)
        deviceOrientation1 = DeviceOrientation.LandscapeLeft;
      else if (num2 > 0.8)
        deviceOrientation1 = DeviceOrientation.LandscapeRight;
      else if (num4 < -0.8)
        deviceOrientation1 = DeviceOrientation.ScreenSideUp;
      else if (num4 > 0.8)
        deviceOrientation1 = DeviceOrientation.ScreenSideDown;
      DeviceOrientation deviceOrientation2 = DeviceOrientation.Unknown;
      bool flag = false;
      if (deviceOrientation1 != DeviceOrientation.Unknown)
      {
        lock (this)
        {
          this._currentOrientation = deviceOrientation1;
          if (this._previousOrientation != this._currentOrientation)
          {
            deviceOrientation2 = this._previousOrientation;
            this._previousOrientation = this._currentOrientation;
            flag = true;
          }
        }
      }
      if (!flag)
        return;
      DeviceOrientationChangedEventArgs e = new DeviceOrientationChangedEventArgs();
      e.CurrentOrientation = deviceOrientation1;
      e.PreviousOrientation = deviceOrientation2;
      // ISSUE: reference to a compiler-generated field
      if (this.OrientationChanged == null)
        return;
      // ISSUE: reference to a compiler-generated field
      this.OrientationChanged(this, e);
    }
  }
}
