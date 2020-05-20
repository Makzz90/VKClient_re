using System;

namespace VKClient.Common.Framework
{
  public class CompressionEventArgs : EventArgs
  {
    public CompressionType Type { get; protected set; }

    public CompressionEventArgs(CompressionType type)
    {
      this.Type = type;
    }
  }
}
