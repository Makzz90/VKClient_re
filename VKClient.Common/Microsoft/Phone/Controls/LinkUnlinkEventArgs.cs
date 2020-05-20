using System;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
  public class LinkUnlinkEventArgs : EventArgs
  {
    public ContentPresenter ContentPresenter { get; private set; }

    public LinkUnlinkEventArgs(ContentPresenter contentPresenter)
    {
      this.ContentPresenter = contentPresenter;
    }
  }
}
