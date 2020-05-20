using System;

namespace XamlAnimatedGif
{
  public class AnimationErrorEventArgs : EventArgs
  {
      public Exception Exception { get; private set; }

      public AnimationErrorKind Kind { get; private set; }

    public AnimationErrorEventArgs(Exception exception, AnimationErrorKind kind)
    {
      this.Exception = exception;
      this.Kind = kind;
    }
  }
}
