using SharpDX.Direct3D11;
using System;

namespace VKClient.Common.Framework.SharpDXExt
{
  public class DeviceResetEventArgs : EventArgs
  {
    public Device Device { get; private set; }

    public DeviceContext DeviceContext { get; private set; }

    internal DeviceResetEventArgs(Device device, DeviceContext context)
    {
      this.Device = device;
      this.DeviceContext = context;
    }
  }
}
