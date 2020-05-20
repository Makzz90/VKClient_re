using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VKClient.Common.Framework;

namespace VKClient.Common.UC.Registration
{
  public class RegistrationStep6UC : UserControl
  {
    internal Grid LayoutRoot;
    internal ExtendedLongListSelector list;
    private bool _contentLoaded;

    public RegistrationStep6UC()
    {
      //base.\u002Ector();
      this.InitializeComponent();
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent(this, new Uri("/VKClient.Common;component/UC/Registration/RegistrationStep6UC.xaml", UriKind.Relative));
      this.LayoutRoot = (Grid) base.FindName("LayoutRoot");
      this.list = (ExtendedLongListSelector) base.FindName("list");
    }
  }
}
