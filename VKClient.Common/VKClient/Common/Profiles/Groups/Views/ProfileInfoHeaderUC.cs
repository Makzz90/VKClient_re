using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VKClient.Common.Profiles.Groups.Views
{
  public class ProfileInfoHeaderUC : UserControl
  {
    private bool _contentLoaded;

    public event EventHandler<GestureEventArgs> PhotoTapped;

    public ProfileInfoHeaderUC()
    {
      this.InitializeComponent();
    }

    private void GridPhoto_OnTapped(object sender, GestureEventArgs e)
    {
      if (this.PhotoTapped == null)
        return;
      this.PhotoTapped(sender, e);
    }

    [DebuggerNonUserCode]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/VKClient.Common;component/Profiles/Groups/Views/ProfileInfoHeaderUC.xaml", UriKind.Relative));
    }
  }
}
