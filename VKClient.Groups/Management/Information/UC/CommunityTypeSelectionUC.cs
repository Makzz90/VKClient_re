using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VKClient.Groups.Management.Information.UC
{
  public class CommunityTypeSelectionUC : UserControl
  {
    private bool _contentLoaded;

    public CommunityTypeSelectionUC()
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
      Application.LoadComponent(this, new Uri("/VKClient.Groups;component/Management/Information/UC/CommunityTypeSelectionUC.xaml", UriKind.Relative));
    }
  }
}
