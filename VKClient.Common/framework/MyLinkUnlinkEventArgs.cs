using System;
using System.Windows.Controls;

namespace VKClient.Common.Framework
{
  public class MyLinkUnlinkEventArgs : EventArgs
  {
    public ContentPresenter ContentPresenter { get; private set; }

    public MyLinkUnlinkEventArgs(ContentPresenter contentPresenter)
    {
      this.ContentPresenter = contentPresenter;
    }
  }
}
