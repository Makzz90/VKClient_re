using System;

namespace ExifLib
{
  [Flags]
  public enum ExifFlash
  {
    No = 0,
    Fired = 1,
    StrobeReturnLightDetected = 6,
    On = 8,
    Off = 16,
    Auto = Off | On,
    FlashFunctionPresent = 32,
    RedEyeReduction = 64,
  }
}
