using System;

namespace VKClient.Common.ImageViewer
{
  [Flags]
  public enum RectSides
  {
    None = 0,
    Left = 1,
    Top = 2,
    Right = 4,
    Bottom = 8,
  }
}
