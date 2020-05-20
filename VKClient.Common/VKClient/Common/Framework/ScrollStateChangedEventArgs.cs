using System;

namespace VKClient.Common.Framework
{
  public class ScrollStateChangedEventArgs : EventArgs
  {
    public bool IsScrolling { get; private set; }

    public ScrollStateChangedEventArgs(bool isScrolling)
    {
      this.IsScrolling = isScrolling;
    }
  }
}
