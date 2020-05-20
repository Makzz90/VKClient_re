using System;

namespace VKMessenger.Framework
{
  public class ContentEventArgs : EventArgs
  {
    private object msg;

    public object Message
    {
      get
      {
        return this.msg;
      }
      set
      {
        this.msg = value;
      }
    }

    public ContentEventArgs(object data)
    {
      this.msg = data;
    }
  }
}
